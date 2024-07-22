using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestList : MonoBehaviour, ISavable
{
    List<Quest> quests = new List<Quest>();

    public event Action onUpdated;

    public void AddQuest(Quest quest)
    {
        if (!quests.Contains(quest))
            quests.Add(quest);

        onUpdated?.Invoke();
    }
    /// <summary>
    /// 通過任務名字 判斷是否任務開始
    /// </summary>
    /// <param name="questName"></param>
    /// <returns></returns>
    public bool IsStarted(string questName)
    {
        var questStatus = quests.FirstOrDefault(q => q.Base.Name == questName)?.Status;
        return questStatus == QuestStatus.Started || questStatus == QuestStatus.Completed;
    }
    /// <summary>
    /// 通過任務名字 判斷是否任務完成
    /// </summary>
    /// <param name="questName"></param>
    /// <returns></returns>
    public bool IsCompleted(string questName)
    {
        var questStatus = quests.FirstOrDefault(q => q.Base.Name == questName)?.Status;
        return questStatus == QuestStatus.Completed;
    }

    /// <summary>
    /// 返回PlayerController的QuestList組件
    /// </summary>
    /// <returns></returns>
    public static QuestList GetQuestList()
    {
        return FindObjectOfType<PlayerController>().GetComponent<QuestList>();
    }

    public object CaptureState()
    {
        //將List<Quest> 的資料轉換成QuestSavaData
        return quests.Select(q => q.GetSaveData()).ToList();
    }

    public void RestoreState(object state)
    {
        var saveData = state as List<QuestSavaData>;
        if (saveData != null)
        {
            //將QuestSavaData 還原成 Quest list
            quests = saveData.Select(q => new Quest(q)).ToList();
            onUpdated?.Invoke();
        }
    }
}
