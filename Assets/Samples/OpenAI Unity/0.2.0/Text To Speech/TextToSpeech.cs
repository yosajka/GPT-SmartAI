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

        private AudioSource audioSource;
        
        private OpenAIApi openai = new OpenAIApi();

        public static TextToSpeech Instance;
        private void Awake() 
        {
            if (Instance == null) Instance = this;
            else Destroy(this);    
        }

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }



        public async void PlayReplyAudio(string text)
        {
            var request = new CreateTextToSpeechRequest
            {
                Input = text,
                Model = modelDropdown.options[modelDropdown.value].text.ToLower(),
                Voice = voiceDropdown.options[voiceDropdown.value].text.ToLower()
            };

            
            var response = await openai.CreateTextToSpeech(request);
            
            
            if(response.AudioClip) 
            {
                StartCoroutine(PlayAnimation());
                audioSource.PlayOneShot(response.AudioClip);
            }
        }

        private IEnumerator PlayAnimation()
        {
            animator.SetBool("Talking", true);
            yield return new WaitUntil(() => !audioSource.isPlaying);
            animator.SetBool("Talking", false);
        }
    }
}
