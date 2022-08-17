
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//this class would be better with animator
public class PostProcessing : MonoBehaviour
{

    Volume postProcessVolume;
    Volume PostProcessVolume
    {
        get => postProcessVolume ? postProcessVolume : postProcessVolume = GetComponent<Volume>();
    }

    DepthOfField depthOfField;
    DepthOfField DepthOfField
    {
        get
        {
            if (depthOfField != null) 
                return depthOfField;
            
            PostProcessVolume.sharedProfile.TryGet(out DepthOfField depthOfFieldComponent);
            depthOfField = depthOfFieldComponent;

            return depthOfField;
        }
    }
    
    LiftGammaGain liftGammaGain;
    LiftGammaGain LiftGammaGain
    {
        get
        {
            if (liftGammaGain != null) 
                return liftGammaGain;
            
            PostProcessVolume.sharedProfile.TryGet(out LiftGammaGain liftGammaGainComponent);
            liftGammaGain = liftGammaGainComponent;

            return liftGammaGain;
        }
    }

    void Awake()
    {
        DepthOfField.active = false;
        LiftGammaGain.active = false;
        DepthOfField.focusDistance.value = 1;
        LiftGammaGain.gain.value = new Vector4(1, 1, 1, 0);
    }

    public void SetBlurBackground(float duration, Action finished = null)
    {
        StartCoroutine(SetBlurBackgroundRoutine(duration, finished));
    }

    public void SetCommonBackground(float duration, Action finished = null)
    {
        StartCoroutine(SetCommonBackgroundRoutine(duration, finished));
    }

    IEnumerator SetBlurBackgroundRoutine(float duration, Action finished)
    {
        DepthOfField.active = true;
        LiftGammaGain.active = true;
        
        float time = 0;
        
        float sourceFocusDistance = DepthOfField.focusDistance.value;
        Vector4 sourceGain = LiftGammaGain.gain.value;
        
        float targetFocusDistance = 0.1f;
        Vector4 targetGain = new (1,1,1,-0.2f);

        while (time < duration)
        {
            yield return null;
            time += Time.unscaledDeltaTime;

            float phase = time / duration;

            DepthOfField.focusDistance.value = Mathf.Lerp(sourceFocusDistance, targetFocusDistance, phase);
            LiftGammaGain.gain.value = Vector4.Lerp(sourceGain, targetGain, phase);
        }
        
        DepthOfField.focusDistance.value = targetFocusDistance;
        LiftGammaGain.gain.value = targetGain;
        
        finished?.Invoke();
    }
    
    IEnumerator SetCommonBackgroundRoutine(float duration, Action finished)
    {
        float time = 0;
        
        float sourceFocusDistance = DepthOfField.focusDistance.value;
        Vector4 sourceGain = LiftGammaGain.gain.value;
        
        float targetFocusDistance = 1f;
        Vector4 targetGain = new (1,1,1,0f);

        while (time < duration)
        {
            yield return null;
            time += Time.unscaledDeltaTime;

            float phase = time / duration;

            DepthOfField.focusDistance.value = Mathf.Lerp(sourceFocusDistance, targetFocusDistance, phase);
            LiftGammaGain.gain.value = Vector4.Lerp(sourceGain, targetGain, phase);
        }
        
        DepthOfField.focusDistance.value = targetFocusDistance;
        LiftGammaGain.gain.value = targetGain;
        DepthOfField.active = false;
        LiftGammaGain.active = false;
        
        finished?.Invoke();
    }
}
