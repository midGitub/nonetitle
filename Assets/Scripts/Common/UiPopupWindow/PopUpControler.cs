using UnityEngine;
using UnityEngine.UI;

public abstract class PopUpControler : MonoBehaviour
{
    public WindowInfo WindowInfo;
    public Canvas CanvasComponent;
    private bool _isInited;

    public virtual void Init()
    {
        if (CanvasComponent != null && CanvasComponent.worldCamera == null)
        {
            CanvasComponent.worldCamera = Camera.main;
            CanvasComponent.gameObject.SetActive(false);
        }

        if (WindowInfo == null)
        {
            WindowInfo = new WindowInfo(() => { CanvasComponent.gameObject.SetActive(true); }, null, CanvasComponent, Close);
        }

        InitText();
        _isInited = true;
    }

    public virtual void Open()
    {
        if (!_isInited)
        {
            Init();
        }

        WindowManager.Instance.ApplyToOpen(WindowInfo);
    }

    protected virtual void InitText()
    {
    }

    public virtual void Close()
    {
        CanvasComponent.gameObject.SetActive(false);
        WindowManager.Instance.TellClosed(WindowInfo);
    }

    protected void RegisterCloseButton(Button button)
    {
        button.onClick.AddListener(Close);
    }

    protected void UnRegisterCloseButton(Button button)
    {
        button.onClick.RemoveListener(Close);
    }
}
