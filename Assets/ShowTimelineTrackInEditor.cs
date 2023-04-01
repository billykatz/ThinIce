
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Timeline;


[MenuEntry("Custom Actions/Show In Editor")]
public class ShowTrackInEditorAction : TrackAction
{
    [TimelineShortcut("ShowTrackInEditorAction", KeyCode.H)]
    public static void HandleShortCut(ShortcutArguments args)
    {
        Invoker.InvokeWithSelectedTracks<ShowTrackInEditorAction>();
    }

    public override bool Execute(IEnumerable<TrackAsset> tracks)
    {
        TrackAsset[] trackArray = tracks.ToArray();
        for (int i = 0; i < trackArray.Length; i++)
        {
            trackArray[i].hideFlags = HideFlags.HideInInspector;
            EditorUtility.SetDirty(trackArray[i]);
        }

        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return true;
    }

    public override ActionValidity Validate(IEnumerable<TrackAsset> tracks)
    {
        return ActionValidity.Valid;
    }
}

#endif