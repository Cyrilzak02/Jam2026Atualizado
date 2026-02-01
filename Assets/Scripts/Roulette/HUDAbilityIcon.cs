using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HUDAbilityIcon : MonoBehaviour
{
    [Header("UI References")]
    public Image abilityIcon;             // arraste a imagem no inspector
    public HUDAbilityIconPop hudAbilityIconPop; // referência à animação de pop

    [Header("Rotation")]
    public float initialSpeed = 720f;    // graus/segundo ao começar
    public float deceleration = 360f;    // graus/segundo²

    [Header("Sprites")]
    public Sprite rabbitSprite;
    public Sprite snakeSprite;
    public Sprite monkeySprite;
    public Sprite dragonSprite;

    private float currentAngle = 0f;
    private float targetAngle = 0f;
    private float currentSpeed = 0f;

    private AbilityType lastAbility = AbilityType.None;

    private Dictionary<AbilityType, float> abilityAngles = new Dictionary<AbilityType, float>()
    {
        { AbilityType.None, 0f },
        { AbilityType.DoubleJump, 135f },
        { AbilityType.Dash, 225f },
        { AbilityType.WallJump, 315f },
        { AbilityType.Glide, 45f }
    };

    private Dictionary<AbilityType, Sprite> abilitySprites;

    void Awake()
    {
        if (abilityIcon == null)
            abilityIcon = GetComponent<Image>();

        if (hudAbilityIconPop == null)
            hudAbilityIconPop = GetComponent<HUDAbilityIconPop>();

        abilitySprites = new Dictionary<AbilityType, Sprite>()
        {
            { AbilityType.DoubleJump, rabbitSprite },
            { AbilityType.Dash, snakeSprite },
            { AbilityType.WallJump, monkeySprite },
            { AbilityType.Glide, dragonSprite }
        };

        currentAngle = abilityIcon.rectTransform.eulerAngles.z;
        targetAngle = currentAngle;
    }

    void Update()
    {
        RotateIcon();
    }

    private void RotateIcon()
    {
        float delta = Mathf.DeltaAngle(currentAngle, targetAngle);

        if (Mathf.Abs(delta) > 0.01f)
        {
            float decelStep = deceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, Mathf.Abs(delta) / Time.deltaTime);
            currentSpeed = Mathf.Max(currentSpeed - decelStep, 0f);

            float step = Mathf.Sign(delta) * currentSpeed * Time.deltaTime;
            if (Mathf.Abs(step) > Mathf.Abs(delta))
                step = delta;

            currentAngle += step;
            abilityIcon.rectTransform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
        }
        else
        {
            currentAngle = targetAngle;
            abilityIcon.rectTransform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
            currentSpeed = 0f;
        }
    }

    /// <summary>
    /// Chame ao mudar a habilidade equipada
    /// </summary>
    public void SetAbility(AbilityType ability)
    {
        if (ability == lastAbility) return; // evita chamada repetida

        lastAbility = ability;

        // atualiza rotação
        if (!abilityAngles.TryGetValue(ability, out float angle))
            angle = 0f;
        targetAngle = angle;
        currentSpeed = initialSpeed;

        // atualiza sprite com pop
        if (ability == AbilityType.None)
        {
            // se não tem habilidade, apenas desaparece o ícone
            hudAbilityIconPop.HideAbilityIcon();
        }
        else if (abilitySprites.TryGetValue(ability, out Sprite sprite) && sprite != null)
        {
            // mostra o ícone correspondente
            hudAbilityIconPop.ShowAbilityIcon(sprite);
        }
    }

}
