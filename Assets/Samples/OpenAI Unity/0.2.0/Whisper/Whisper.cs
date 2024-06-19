using OpenAI;
using UnityEngine;
using UnityEngine.UI;

namespace Samples.Whisper
{
    public class Whisper : MonoBehaviour
    {
        [SerializeField] private Button recordButton;
        [SerializeField] private Button stopRecordButton;
        [SerializeField] private Image progressBar;
        [SerializeField] private Text message;
        [SerializeField] private Dropdown dropdown;
        [SerializeField] private GameObject settingPanel;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button closeSettingButton;
        
        private readonly string fileName = "output.wav";
        private readonly int duration = 30;
        
        private AudioClip clip;
        private bool isRecording;
        private float time;
        private OpenAIApi openai = new OpenAIApi();

        private void Start()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            dropdown.options.Add(new Dropdown.OptionData("Microphone not supported on WebGL"));
            #else
            foreach (var device in Microphone.devices)
            {
                dropdown.options.Add(new Dropdown.OptionData(device));
            }
            recordButton.onClick.AddListener(StartRecording);
            dropdown.onValueChanged.AddListener(ChangeMicrophone);
            settingButton.onClick.AddListener(OpenSetting);
            closeSettingButton.onClick.AddListener(CloseSetting);
            stopRecordButton.onClick.AddListener(EndRecording);
            
            var index = PlayerPrefs.GetInt("user-mic-device-index");
            dropdown.SetValueWithoutNotify(index);
            #endif
        }

        private void ChangeMicrophone(int index)
        {
            PlayerPrefs.SetInt("user-mic-device-index", index);
        }
        
        private void StartRecording()
        {
            recordButton.gameObject.SetActive(false);
            stopRecordButton.gameObject.SetActive(true);

            isRecording = true;
            recordButton.enabled = false;

            var index = PlayerPrefs.GetInt("user-mic-device-index");
            
            #if !UNITY_WEBGL
            clip = Microphone.Start(dropdown.options[index].text, false, duration, 44100);
            #endif
        }

        private async void EndRecording()
        {
            recordButton.gameObject.SetActive(true);
            stopRecordButton.gameObject.SetActive(false);
            isRecording = false;
            
            #if !UNITY_WEBGL
            Microphone.End(null);
            #endif
            
            byte[] data = SaveWav.Save(fileName, clip);
            
            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() {Data = data, Name = "audio.wav"},
                // File = Application.persistentDataPath + "/" + fileName,
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);

            progressBar.fillAmount = 0;
            ChatGPT.Instance.SendReply(res.Text);
            recordButton.enabled = true;

            
        }

        private void Update()
        {
            if (isRecording)
            {
                time += Time.deltaTime;
                progressBar.fillAmount = time / duration;
                
                if (time >= duration)
                {
                    time = 0;
                    isRecording = false;
                    EndRecording();
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (settingPanel.activeSelf)
                {
                    CloseSetting();
                }
                else
                {
                    OpenSetting();
                }
            }
        }

        private void OpenSetting()
        {
            Debug.Log("haha ");
            settingPanel.SetActive(true);
            settingButton.gameObject.SetActive(false);
        }

        private void CloseSetting()
        {
            settingPanel.SetActive(false);
            settingButton.gameObject.SetActive(true);
        }
    }
}
