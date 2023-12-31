using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//タッチ処理を行う
//今のところ判定できるのは．
//押下されているかどうか・タップ・ロングタップ・左右フリック
public class TouchHandler : MonoBehaviour
{
    private Configuration config;
    InputSettings inputSettings;
    public bool _pressed = false;
    public bool _tapped = false;
    public bool _longtapped = false;
    public bool _flickLtoR = false;
    public bool _flickRtoL = false;
    public Vector2 position;
    public Vector2 startposition;
    private bool _tapmoved = false;
    private Vector2 maxPosition;
    private Vector2 minPosition;
    private float tapTime = 0.0f;
    DebugControl debugControl;

    void Start()
    {
        inputSettings = ScriptableObject.CreateInstance("InputSettings") as InputSettings;
        config = GameObject.Find("Configuration").GetComponent<Configuration>();
        Debug.Log(inputSettings.tapRadius);
        maxPosition = new Vector2(0.0f, 0.0f);
        minPosition = new Vector2(config.displayWidth, config.displayHeight);
        position = new Vector2(0.0f, 0.0f);
        debugControl = GameObject.Find("MainCamera").GetComponent<DebugControl>();
    }

    void Update()
    {
        //タッチ情報を取得
        var touchInput = Touchscreen.current;
        //1つ目のタッチ情報のみを処理
        var primaryTouch = touchInput.touches[0];

        //タッチされた場合
        if (primaryTouch.press.wasPressedThisFrame)
        {
            _tapped = false;
            _pressed = true;
            _tapmoved = false;
            tapTime = 0.0f;
            _flickLtoR = false;
            _flickRtoL = false;
            maxPosition = new Vector2(0.0f, 0.0f);
            minPosition = new Vector2(config.displayWidth, config.displayHeight);
            position = primaryTouch.position.ReadValue() - new Vector2(config.displayWidth / 2.0f, config.displayHeight / 2.0f);
            startposition = primaryTouch.startPosition.ReadValue() - new Vector2(config.displayWidth / 2.0f, config.displayHeight / 2.0f);
        }
        //タッチしてから離された場合
        if (primaryTouch.press.wasReleasedThisFrame)
        {
            _pressed = false;
            _tapmoved = false;
            _longtapped = false;
            //フリック判定
            //ある程度横方向に移動しており時間が短ければフリック判定
            if (tapTime <= config.flickTime && Mathf.Abs(maxPosition.y - minPosition.y) <= config.flickMoveY && Mathf.Abs(maxPosition.x - minPosition.x) >= config.flickMoveX)
            {
                if (primaryTouch.startPosition.ReadValue().x <= config.flickStartPosL)
                {
                    _flickLtoR = true;
                    debugControl.AddLog("FlickLtoR");
                }
                if (primaryTouch.startPosition.ReadValue().x >= config.flickStartPosR)
                {
                    _flickRtoL = true;
                    debugControl.AddLog("FlickRtoL");
                }
            }
            //tap判定
            if (tapTime <= inputSettings.defaultTapTime && !_tapmoved)
            {
                _tapped = true;
                debugControl.AddLog("Tap");
            }
        }
        //押されている時間の計算
        if (_pressed)
        {
            tapTime += Time.deltaTime;
            position = primaryTouch.position.ReadValue() - new Vector2(config.displayWidth/2.0f,config.displayHeight/2.0f);
            //押されている間に通った場所の最大値最小値を記録
            if (maxPosition.x <= primaryTouch.position.ReadValue().x) maxPosition.x = primaryTouch.position.ReadValue().x;
            if (minPosition.x >= primaryTouch.position.ReadValue().x) minPosition.x = primaryTouch.position.ReadValue().x;
            if (maxPosition.y <= primaryTouch.position.ReadValue().y) maxPosition.y = primaryTouch.position.ReadValue().y;
            if (minPosition.y >= primaryTouch.position.ReadValue().y) minPosition.y = primaryTouch.position.ReadValue().y;
        }
        //tap移動判定
        if (_pressed && !_tapmoved)
        {
            if (Vector2.Distance(primaryTouch.startPosition.ReadValue(), primaryTouch.position.ReadValue()) > inputSettings.tapRadius)
            {
                _tapmoved = true;
                debugControl.AddLog("tap Moved");
            }
        }
        //logtap判定
        if (_pressed)
        {
            if (!_tapmoved && tapTime >= config.longTapTime && !_longtapped)
            {
                _longtapped = true;
                debugControl.AddLog("Long tap");
            }
        }
        
    }

    public void ResetTap()
    {
        _tapped = false;
    }

    public void ResetLongTap()
    {
        _longtapped = false;
    }
}
