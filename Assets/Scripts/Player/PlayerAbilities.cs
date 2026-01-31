using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum AbilityType
{
    None,
    DoubleJump,
    Dash,
    WallJump,
    Glide
}

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerAbilities : MonoBehaviour
{
    [Header("Refs")]
    public PlayerController2D playerController; // arraste no Inspector

    [Header("Cooldowns (s)")]
    public float doubleJumpCooldown = 5f;
    public float dashCooldown = 3f;
    public float wallJumpCooldown = 4f;
    public float glideCooldown = 6f;

    [Header("Glide Settings")]
    public float glideMaxDuration = 3f;

    [Header("Colors")]
    public Color colorWallJump = new Color(1f, 0.84f, 0f); // dourado
    public Color colorDoubleJump = new Color(1f, 0.76f, 0.80f); // pêssego
    public Color colorGlide = Color.red;
    public Color colorDash = new Color(0f, 0.4f, 0f); // verde escuro
    public Color colorNone = Color.white;

    private SpriteRenderer sr;
    private PlayerInput input;
    private AbilityType equipped = AbilityType.None;
    private Dictionary<AbilityType, float> cooldowns = new Dictionary<AbilityType, float>();

    // glide control
    private Coroutine glideRoutine;
    private float glideRemaining;

    // input actions
    private InputAction equipDoubleJumpAction;
    private InputAction equipDashAction;
    private InputAction equipWallJumpAction;
    private InputAction equipGlideAction;
    private InputAction useAbilityAction;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        input = GetComponent<PlayerInput>();
        if (playerController == null)
            playerController = GetComponent<PlayerController2D>();

        // cooldowns init
        cooldowns[AbilityType.DoubleJump] = 0f;
        cooldowns[AbilityType.Dash] = 0f;
        cooldowns[AbilityType.WallJump] = 0f;
        cooldowns[AbilityType.Glide] = 0f;
    }

    // métodos usados nos callbacks (evitam lambdas anônimas)
    private void OnEquipDoubleJump(InputAction.CallbackContext ctx) => TryEquip(AbilityType.DoubleJump);
    private void OnEquipDash(InputAction.CallbackContext ctx) => TryEquip(AbilityType.Dash);
    private void OnEquipWallJump(InputAction.CallbackContext ctx) => TryEquip(AbilityType.WallJump);
    private void OnEquipGlide(InputAction.CallbackContext ctx) => TryEquip(AbilityType.Glide);

    void OnEnable()
    {
        // ativa o mapa de inputs
        input = GetComponent<PlayerInput>();
        if (input == null)
        {
            Debug.LogError("[PlayerAbilities] PlayerInput not found!");
            return;
        }

        // Mostra todos os nomes disponíveis para você confirmar
        foreach (var map in input.actions.actionMaps)
        {
            Debug.Log($"[PlayerAbilities] Found Action Map: {map.name}");
            foreach (var act in map.actions)
                Debug.Log($"   → Action: {act.name}");
        }

        // Agora tente localizar automaticamente (sem precisar saber o nome do mapa)
        equipDoubleJumpAction = input.actions.FindAction("Double", false);
        equipDashAction        = input.actions.FindAction("Dash", false);
        equipWallJumpAction    = input.actions.FindAction("Wall", false);
        equipGlideAction       = input.actions.FindAction("Glide", false);
        useAbilityAction       = input.actions.FindAction("Use", false);

        // Ativa e registra apenas as ações que foram encontradas
        if (equipDoubleJumpAction != null) { equipDoubleJumpAction.Enable(); equipDoubleJumpAction.performed += OnEquipDoubleJump; }
        if (equipDashAction != null)       { equipDashAction.Enable(); equipDashAction.performed += OnEquipDash; }
        if (equipWallJumpAction != null)   { equipWallJumpAction.Enable(); equipWallJumpAction.performed += OnEquipWallJump; }
        if (equipGlideAction != null)      { equipGlideAction.Enable(); equipGlideAction.performed += OnEquipGlide; }
        if (useAbilityAction != null)      
        { 
            useAbilityAction.Enable(); 
            useAbilityAction.performed += OnUsePressed; 
            useAbilityAction.canceled += OnUseReleased; 
        }

        Debug.Log("[PlayerAbilities] Input actions connected.");

        if (playerController != null)
            playerController.OnJumped += OnPlayerJumped;
    }

    void OnDisable()
    {
        // remove eventos com segurança
        if (equipDoubleJumpAction != null) equipDoubleJumpAction.performed -= OnEquipDoubleJump;
        if (equipDashAction != null) equipDashAction.performed -= OnEquipDash;
        if (equipWallJumpAction != null) equipWallJumpAction.performed -= OnEquipWallJump;
        if (equipGlideAction != null) equipGlideAction.performed -= OnEquipGlide;

        if (useAbilityAction != null)
        {
            useAbilityAction.performed -= OnUsePressed;
            useAbilityAction.canceled -= OnUseReleased;
        }

        if (playerController != null)
            playerController.OnJumped -= OnPlayerJumped;
    }



    void Update()
    {
        // reduzir cooldowns
        var keys = new List<AbilityType>(cooldowns.Keys);
        foreach (var k in keys)
            if (cooldowns[k] > 0f)
                cooldowns[k] -= Time.deltaTime;
    }

    // ============================================
    // EQUIP / UNEQUIP
    // ============================================

    public bool IsEquipped(AbilityType type)
    {
        return equipped == type;
    }

    public void TryEquip(AbilityType type)
    {
        if (type == AbilityType.None) return;
        if (cooldowns[type] > 0f)
        {
            Debug.Log($"{type} em cooldown ({cooldowns[type]:F1}s)");
            return;
        }

        // Se já estiver equipada, desativa
        if (equipped == type)
        {
            Unequip();
            return;
        }

        // --- Faz o swap entre habilidades ---
        Unequip(); // desativa a habilidade anterior
        equipped = type;
        ApplyColor(type);

        switch (type)
        {
            case AbilityType.DoubleJump:
                // Se estava planando, para o glide e dá pulo no ar
                playerController.SetExternalGlide(false);
                playerController.EnableWallGrip(false);
                playerController.GrantAirDoubleJump();
                break;

            case AbilityType.Glide:
                // Remove qualquer pulo extra e garante que só planará segurando espaço
                playerController.ClearAirJumps();
                playerController.EnableWallGrip(false);
                break;

            case AbilityType.WallJump:
                // Ativa o grip e desativa glide/pulos extras
                playerController.EnableWallGrip(true);
                playerController.ClearAirJumps();
                playerController.SetExternalGlide(false);
                break;

            case AbilityType.Dash:
                // Apenas certifique que não há glide ativo
                playerController.SetExternalGlide(false);
                playerController.EnableWallGrip(false);
                playerController.ClearAirJumps();
                break;
        }

        Debug.Log($"Equipped {type}");
    }

    void Unequip()
    {
        if (equipped == AbilityType.None) return;

        switch (equipped)
        {
            case AbilityType.WallJump:
                playerController.EnableWallGrip(false);
                break;

            case AbilityType.Glide:
                playerController.SetExternalGlide(false);
                break;

            case AbilityType.DoubleJump:
                playerController.ClearAirJumps();
                break;
        }

        equipped = AbilityType.None;
        ApplyColor(AbilityType.None);
    }

    public void ForceUnequip()
    {
        Unequip();
    }



    void ApplyColor(AbilityType type)
    {
        switch (type)
        {
            case AbilityType.DoubleJump: sr.color = colorDoubleJump; break;
            case AbilityType.Dash: sr.color = colorDash; break;
            case AbilityType.WallJump: sr.color = colorWallJump; break;
            case AbilityType.Glide: sr.color = colorGlide; break;
            default: sr.color = colorNone; break;
        }
    }

    // ============================================
    // ATIVAÇÃO
    // ============================================
    void OnUsePressed(InputAction.CallbackContext ctx)
    {
        if (equipped == AbilityType.None) return;

        switch (equipped)
        {
            case AbilityType.DoubleJump:
                if (!playerController.isGrounded)
                {
                    playerController.ForceDoubleJump();
                    StartCooldown(AbilityType.DoubleJump);
                }
                break;

            case AbilityType.Dash:
                playerController.StartDashFromAbility();
                StartCooldown(AbilityType.Dash);
                break;

            case AbilityType.WallJump:
                playerController.EnableWallGrip(false);
                StartCooldown(AbilityType.WallJump);
                break;

            case AbilityType.Glide:
                if (glideRoutine == null)
                    glideRoutine = StartCoroutine(GlideHold());
                break;
        }
    }

    void OnUseReleased(InputAction.CallbackContext ctx)
    {
        if (equipped == AbilityType.Glide && glideRoutine != null)
        {
            StopCoroutine(glideRoutine);
            glideRoutine = null;
            EndGlide();
        }
    }

    IEnumerator GlideHold()
    {
        glideRemaining = glideMaxDuration;
        playerController.SetExternalGlide(true);

        while (glideRemaining > 0f)
        {
            glideRemaining -= Time.unscaledDeltaTime;
            yield return null;
        }

        EndGlide();
    }

    void EndGlide()
    {
        playerController.SetExternalGlide(false);
        glideRoutine = null;
        StartCooldown(AbilityType.Glide);
    }

    // ============================================
    // COOLDOWN / EVENTOS
    // ============================================
    void StartCooldown(AbilityType a)
    {
        float cd = 1f;
        switch (a)
        {
            case AbilityType.DoubleJump: cd = doubleJumpCooldown; break;
            case AbilityType.Dash: cd = dashCooldown; break;
            case AbilityType.WallJump: cd = wallJumpCooldown; break;
            case AbilityType.Glide: cd = glideCooldown; break;
        }
        cooldowns[a] = cd;

        if (equipped == a)
            Unequip();
    }

    void OnPlayerJumped()
    {
        if (equipped == AbilityType.WallJump)
            StartCooldown(AbilityType.WallJump);
    }

    public string GetEquippedName()
    {
        return equipped.ToString();
    }

}
