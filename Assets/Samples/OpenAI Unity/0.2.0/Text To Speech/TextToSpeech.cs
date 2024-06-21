using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace OpenAI
{
    [RequireComponent(typeof(AudioSource))]
    public class TextToSpeech : MonoBehaviour
    {
        //[SerializeField] private InputField inputField;
        [SerializeField] private Dropdown voiceDropdown;
        [SerializeField] private Dropdown modelDropdown;
        [SerializeField] private Animator animator;
        //[SerializeField] private Button button;
        //[SerializeField] private Text buttonText;

        [SerializeField] private AudioSource audioSource;
        
        private OpenAIApi openai;

        public static TextToSpeech Instance;
        private void Awake() 
        {
            if (Instance == null) Instance = this;
            else Destroy(this);    
        }

        private void Start()
        {
            //audioSource = GetComponent<AudioSource>();
        }



        public async void PlayReplyAudio(string text)
        {
            if (openai == null)
            {
                openai = new OpenAIApi(SaveSystem.Instance.PlayerData.apiKey);
            }
            var request = new CreateTextToSpeechRequest
            {
                Input = text,
                Model = modelDropdown.options[modelDropdown.value].text.ToLower(),
                Voice = voiceDropdown.options[voiceDropdown.value].text.ToLower()
            };

            
            var response = await openai.CreateTextToSpeech(request);
            
            
            if(response.AudioClip) 
            {
                StartCoroutine(PlayAnimation(response.AudioClip.length));
                audioSource.clip = response.AudioClip;
                audioSource.Play();
                //animator.SetTrigger("Talk");
            }
        }

        private IEnumerator PlayAnimation(float length)
        {
            animator.SetTrigger("Talk");
            var waitForClipRemainingTime = new WaitForSeconds(length);
            yield return waitForClipRemainingTime;
            animator.SetTrigger("Talk");
        }
    }
}
