using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper
{
    public static IEnumerator Fade(CanvasGroup canvasGroup, float valorDeseado, float tiempoFade)
    {
        float timer = 0;
        float valorInicial = canvasGroup.alpha;
        while (timer < tiempoFade)
        {
            canvasGroup.alpha = Mathf.Lerp(valorInicial, valorDeseado, timer / tiempoFade);
            timer += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = valorDeseado;
    }
}