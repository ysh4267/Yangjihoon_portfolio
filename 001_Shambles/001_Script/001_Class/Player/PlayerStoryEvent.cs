using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStoryEvent
{
    public List<StoryEvent> storyEventIndexQueueList;
    public int? currentAreaIndex;
    public int? currentEventIndex;
    public int? currentBattleIndex;
    public List<int> selectedStorySelectionIndexList;
    public int? currentEventCount;
    public List<int> currentEndingIndexList;
    public int? currentDeadEndingIndex;
    public int? currentDeadEndingSentenceIndex;
    public List<int> currentRevealedAreaIndexList;
    public int? currentEncounterRerollCount;

    public PlayerStoryEvent() {
        storyEventIndexQueueList = new List<StoryEvent>(); 
        selectedStorySelectionIndexList = new List<int>();
        currentEndingIndexList = new List<int>();
        currentRevealedAreaIndexList = new List<int>();
    }
}
