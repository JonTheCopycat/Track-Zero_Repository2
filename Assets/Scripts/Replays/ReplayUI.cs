using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UISystems;
using UnityEngine.EventSystems;

namespace Replays
{
    public class ReplayUI : MonoBehaviour
    {
        [SerializeField]
        GameObject listItem;

        [SerializeField]
        Selectable backButton;

        List<GameObject> ReplayFiles;

        // Start is called before the first frame update
        void Start()
        {
            ReplayFiles = new List<GameObject>();

            if (ReplayStorage.allReplays.Count == 0)
                ReplayStorage.LoadAllReplays();

            ReplayFileDisplay currentItem;
            ReplayFileDisplay lastItem = null;
            for (int i = 0; i < ReplayStorage.allReplays.Count; i++)
            {
                if (ReplayStorage.allReplays[i].GetTrack() == ReplayStorage.selectedTrack)
                {
                    GameObject temp = Instantiate(listItem);
                    currentItem = temp.GetComponent<ReplayFileDisplay>();
                    currentItem.SetData(ReplayStorage.allReplays[i]);

                    if (lastItem != null)
                    {
                        currentItem.PreviousFile = lastItem;
                        lastItem.NextFile = currentItem;
                    }
                    else if (i == 0)
                    {
                        currentItem.PreviousFile = backButton;
                        var nav = backButton.navigation;
                        nav.selectOnDown = currentItem;
                    }

                    lastItem = currentItem;
                }
            }
        }

        public void SelectFirstItem()
        {
            //clear selected object
            EventSystem.current.SetSelectedGameObject(null);

            //set a new selected object
            if (ReplayFiles.Count <= 0)
                EventSystem.current.SetSelectedGameObject(backButton.gameObject);
            else
                EventSystem.current.SetSelectedGameObject(ReplayFiles[0]);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
