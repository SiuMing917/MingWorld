using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    List<Sprite> frames;
    float frameRate;

    int currentFrame;
    float timer;

    public SpriteAnimator(List<Sprite> frames, SpriteRenderer spriteRenderer, float frameRate = 0.16f)
    {
        this.frames = frames;
        this.spriteRenderer = spriteRenderer;
        this.frameRate = frameRate;
    }

    public void Start()
    {
        currentFrame = 0;
        timer = 0;
        spriteRenderer.sprite = frames[0];
    }

    /// <summary>
    /// 切换Frame動畫
    /// </summary>
    public void HandleUpdate()
    {
        timer += Time.deltaTime;
        //時間大於Frame Rate
        if (timer > frameRate)
        {
            // 切換到下一張
            //當切換到最後張時 回到第一張
            currentFrame = (currentFrame + 1) % frames.Count;
            spriteRenderer.sprite = frames[currentFrame];
            //這一行代碼將計時器減去幀率(frameRate)，以確保計時器仍然與幀率同步，並控制動畫更新的頻率。
            timer -= frameRate;
        }
    }

    public List<Sprite> Frames
    {
        get
        {
            return frames;
        }
    }
}
