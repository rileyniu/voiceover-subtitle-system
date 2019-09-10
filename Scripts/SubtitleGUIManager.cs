using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Bestiary
{
    public class SubtitleGUIManager : MonoBehaviour
    {
        public Canvas canvas;
        public Text textComponent;
        public Transform rightHandAnchor;
        public Transform centerEyeAnchor;
        private Transform dominantController;
        [SerializeField]
        private float forwardOffset = 0f;
        [SerializeField]
        private float verticalOffset = 0.1f;
        private Vector3 offset;

        private static SubtitleGUIManager instance = null;
        private SubtitleGUIManager() { }
        public static SubtitleGUIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    instance = obj.AddComponent<SubtitleGUIManager>();
                }
                return instance;
            }
        }

        void Awake()
        {
            instance = this;

            // obtain dominant controller to place subtitle box
            if (dominantController == null)
            {
                dominantController = rightHandAnchor;
                // TODO: dominantController = GetDominantController();
            }
            offset = new Vector3(0, verticalOffset, 0) + dominantController.transform.forward * forwardOffset;

            // GUI component initialization
            if (canvas == null) 
            {
                canvas = gameObject.GetComponentInChildren<Canvas>();
                Debug.Assert(canvas != null, "Missing subtitle canvas component.");
            }

            if(textComponent == null)
            {
                textComponent = canvas.GetComponentInChildren<Text>();
                Debug.Assert(textComponent != null, "Missing subtitle text component.");
            }
        }

        void Update()
        {
            Vector3 rotationVector = dominantController.position - centerEyeAnchor.position;
            Quaternion rotation = Quaternion.LookRotation(rotationVector);
            // fix the subtitle box to the dominant controller
            canvas.gameObject.transform.SetPositionAndRotation(dominantController.transform.position + offset, rotation);
        }

        /// <summary>
        /// set the current loaded text of the subtitle box
        /// </summary>
        /// <param name="line"> loaded line from the file</param>
        public void SetText(string line)
        {
            textComponent.text = line;
            
        }

        public void EnableSubtitleCanvas()
        {
            canvas.gameObject.SetActive(true);
        }
        public void DisableSubtitleCanvas()
        {

            canvas.gameObject.SetActive(false);
        }
    }
}