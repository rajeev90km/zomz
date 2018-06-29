using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConversationController : MonoBehaviour {
    
    [SerializeField]
    private GameData _gameData;

    private const int MAX_CHAR_LIMIT = 60;
    private const float AVATAR_FADE_IN_TIME = 1f;
    private const float BG_FADE_IN_TIME = 1.5f;
    private const float BG_FADE_OUT_TIME = 0.75f;
    private const float FADE_BG_OPACTIY = 0.75f;

    [SerializeField]
    [Range(0.01f, 0.1f)]
    private float _characterInterval = 0.05f;

    [Header("UI")]
    [SerializeField]
    private GameObject _conversationCanvas;

    [SerializeField]
    private GameObject _conversationPanel;

    [SerializeField]
    private Image _conversationBg;

    [SerializeField]
    private Image _fadeBg;

    [SerializeField]
    private Text _conversationText;

    [SerializeField]
    private Text _avatarName;

    [SerializeField]
    private Image[] _avatars;

    [Header("Events")]
    [SerializeField]
    private GameEvent _conversationEndEvent;

    private int charactersLeft;
    private List<string> _fullText = new List<string>();
    private int _textIndex;
    private Coroutine _typeTextCoroutine;

    private int currentConversationIndex = 0;

	private void Start()
	{
        
	}

	public void ProcessCurrentConversation()
    {
        currentConversationIndex = 0;
        _conversationCanvas.SetActive(true);
        _gameData.IsPaused = _gameData.CurrentConversation.Conversation.PauseGamePlay;

        BeginProcessingConversation();
    }

    public void EndConversation()
    {
        currentConversationIndex = 0;
        _gameData.CurrentConversation.Conversation = null;
        ToggleDisplayElements(false);

        StartCoroutine(FadeInOutBg(false));
    }

    IEnumerator FadeInOutBg(bool fadeIn)
    {
        float time = 0f;

        Color c = Color.black;

        while (time < 1)
        {
            if(fadeIn)
                c.a = Mathf.Lerp(0, FADE_BG_OPACTIY, time);
            else
                c.a = Mathf.Lerp(FADE_BG_OPACTIY, 0, time);
            _fadeBg.color = c;

            if(fadeIn)
                time += Time.deltaTime / BG_FADE_IN_TIME;
            else
                time += Time.deltaTime / BG_FADE_OUT_TIME;
            yield return null;
        }

        if (fadeIn)
            c.a = 1;
        else
            c.a = 0;
        _fadeBg.color = c;

        // if fadeout at end of convo, raise end flags
        if (!fadeIn)
        {
            _conversationCanvas.SetActive(false);
            _conversationEndEvent.Raise();
        }
        else
        {
            Display(_gameData.CurrentConversation.Conversation.AllDialogues[currentConversationIndex]);
        }

        yield return null;
    }

    void BeginProcessingConversation()
    {
        Color c = Color.black;
        c.a = 0;
        _fadeBg.color = c;

        if(currentConversationIndex==0)
            StartCoroutine(FadeInOutBg(true));
        else
            Display(_gameData.CurrentConversation.Conversation.AllDialogues[currentConversationIndex]);
    }

    void ToggleDisplayElements(bool pEnable)
    {
        _conversationBg.gameObject.SetActive(pEnable);
        _conversationPanel.gameObject.SetActive(pEnable);
        _conversationText.gameObject.SetActive(pEnable);
        _avatarName.gameObject.SetActive(pEnable);

        for (int i = 0; i < _avatars.Length; i++)
        {
            _avatars[i].gameObject.SetActive(pEnable);
        }
    }

    void Display(ConversationEntity pEntity)
    {
        List<string> wordChunks = TokenizeWords(pEntity.Text);
        _fullText = TokensToSentenceChunks(wordChunks);

        ToggleDisplayElements(true);

        _textIndex = 0;
        _avatarName.text = pEntity.CharacterName.ToUpper() + " :";

        Color c = Color.white;
        c.a = 0.5f;
        for (int i = 0; i < _avatars.Length; i++){
            if(_avatars[i].color.a > 0f && i!=(int) pEntity.CharPosition)
                _avatars[i].color = c;
        }

        _avatars[(int)pEntity.CharPosition].sprite = pEntity.Avatar;
        StartCoroutine(FadeInAvatar(_avatars[(int)pEntity.CharPosition]));

        if (pEntity.BackgroundImage != null)
        {
            Color cbg = Color.white;
            cbg.a = 1;
            _conversationBg.sprite = pEntity.BackgroundImage;
            _conversationBg.color = cbg;
        }
        else{
            Color cbg = Color.black;
            cbg.a = FADE_BG_OPACTIY;
            _conversationBg.color = cbg;
        }

        Color fbg = Color.black;
        fbg.a = FADE_BG_OPACTIY;
        _fadeBg.color = fbg;


        RunText(_fullText[_textIndex]);
    }


    IEnumerator FadeInAvatar(Image pCurrentAvatar)
    {
        float time = 0f;

        Color c = Color.white;

        while(time<1)
        {
            c.a = Mathf.Lerp(pCurrentAvatar.color.a, 1, time);
            pCurrentAvatar.color = c;
            time += Time.deltaTime / AVATAR_FADE_IN_TIME;
            yield return null;       
        }

        c.a = 1;
        pCurrentAvatar.color = c;

        yield return null;
    }

    public void DisplayFullText(string pText)
    {
        _conversationText.text = pText;
    }

    void RunText(string pText)
    {
        if (_typeTextCoroutine != null)
        {
            StopCoroutine(_typeTextCoroutine);
            _typeTextCoroutine = null;
        }

        _typeTextCoroutine = StartCoroutine(TypeTextFX(_fullText[_textIndex]));
    }

    IEnumerator TypeTextFX(string pText)
    {
        _conversationText.text = "";

        for (int i = 0; i < pText.Length; i++)
        {
            _conversationText.text += pText[i];
            yield return new WaitForSeconds(_characterInterval);
        }

        _typeTextCoroutine = null;
    }

	
    // Splits given string into words and adds them to a list
    private List<string> TokenizeWords(string pStr)
    {
        return new List<string>(pStr.Split(' '));
    }


    private List<string> TokensToSentenceChunks(List<string> pWordChunks)
    {
        List<string> sentenceChunks = new List<string>();

        while (pWordChunks.Count > 0)
        {
            charactersLeft = MAX_CHAR_LIMIT;
            bool canAddToCurrentText = true;

            string currentString = "";

            while (canAddToCurrentText)
            {
                if (pWordChunks.Count > 0)
                {
                    if (charactersLeft - pWordChunks[0].Length > 0)
                    {
                        if (currentString == "")
                            currentString += pWordChunks[0];
                        else
                            currentString += " " + pWordChunks[0];
                        charactersLeft -= (pWordChunks[0].Length + 1);
                        pWordChunks.RemoveAt(0);
                    }
                    else
                        canAddToCurrentText = false;
                }
                else
                    canAddToCurrentText = false;
            }

            currentString += "...";

            sentenceChunks.Add(currentString);

            if (pWordChunks.Count <= 0)
                break;
        }

        return sentenceChunks;
    }

	void Update () 
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_gameData.CurrentConversation.Conversation != null)
            {
                if (_typeTextCoroutine != null)
                {
                    StopCoroutine(_typeTextCoroutine);
                    _typeTextCoroutine = null;
                    DisplayFullText(_fullText[_textIndex]);
                }
                else
                {
                    _textIndex++;

                    if (_textIndex >= _fullText.Count)
                    {
                        if (currentConversationIndex < _gameData.CurrentConversation.Conversation.AllDialogues.Count - 1)
                        {
                            currentConversationIndex++;
                            BeginProcessingConversation();
                        }
                        else
                            EndConversation();
                    }
                    else
                    {
                        RunText(_fullText[_textIndex]);
                    }
                }
            }
        }	
	}
}
