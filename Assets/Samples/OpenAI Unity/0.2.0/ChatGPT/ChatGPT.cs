using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private InputField inputField;
        [SerializeField] private Button button;
        [SerializeField] private ScrollRect scroll;
        
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;

        [SerializeField] private Dropdown modelDropdown;

        [SerializeField] private Button exitButton;

        private float height;
        private OpenAIApi openai = new OpenAIApi();

        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = "Act as a random stranger in a chat room and reply to the questions. Don't break character. Don't ever mention that you are an AI model.";

        public static ChatGPT Instance;
        private void Awake() 
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            button.onClick.AddListener(() => SendReply());
            exitButton.onClick.AddListener(() => Application.Quit());
        }

        private void Update() 
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendReply();
            } 
                
        }

        public void Init(string _prompt, string key)
        {
            prompt = _prompt;
            openai = new OpenAIApi(key);
            messages.Clear();
            foreach (Transform child in scroll.content.transform)
            {
                Destroy(child.gameObject);
            }
        }

        private void AppendMessage(ChatMessage message)
        {
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

            var item = Instantiate(message.Role == "user" ? sent : received, scroll.content);
            item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;
            item.anchoredPosition = new Vector2(0, -height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            height += item.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;
        }

        public async void SendReply(string sendMessage = "")
        {
            if (sendMessage != "") 
                inputField.text = sendMessage;
            if (inputField.text == "") 
                return;

            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = inputField.text
            };
            
            AppendMessage(newMessage);

            if (messages.Count == 0) 
            {
                newMessage.Content = prompt + "\n" + inputField.text;
            }
            
            messages.Add(newMessage);
            
            button.enabled = false;
            inputField.text = "";
            inputField.enabled = false;
            
            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = modelDropdown.options[modelDropdown.value].text.ToLower(),
                Messages = messages
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();
                TextToSpeech.Instance.PlayReplyAudio(message.Content);
                messages.Add(message);
                AppendMessage(message);
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }

            button.enabled = true;
            inputField.enabled = true;
        }
    }
}
