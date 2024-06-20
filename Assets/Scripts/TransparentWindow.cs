using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class TransparentWindow : MonoBehaviour
{
    const int SWP_HIDEWINDOW = 0x80;
    const int SWP_SHOWWINDOW = 0x40;
    const int SWP_NOMOVE = 0x0002;
    const int SWP_NOSIZE = 0x0001;
    const uint WS_SIZEBOX = 0x00040000;
    const int GWL_STYLE = -16;
    const int WS_MINIMIZE = 0x20000000;
    const int WS_MAXMIZE = 0x01000000;
    const int WS_BORDER = 0x00800000;
    const int WS_DLGFRAME = 0x00400000;
    const int WS_CAPTION = WS_BORDER | WS_DLGFRAME;
    const int WS_SYSMENU = 0x00080000;
    const int WS_MAXIMIZEBOX = 0x00010000;
    const int WS_MINIMIZEBOX = 0x00020000;

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    /// <summary>
    /// window 크기와 위치변경
    /// </summary>
    /// <param name="hWnd">변경할 window handle</param>
    /// <param name="hWndInsertAfter"> Z순서상 변경할 handle 앞에 올 handle</param>
    /// HWND_BOTTOM : Z순서의 맨 아래에 위도우를 놓는다.
    /// HWND_NOTOPMOST : 맨 위에있는 모든 윈도우 뒤에 윈도우를 놓는다.
    /// HWND_TOP : Z순서상 맨 위에 윈도우를 놓는다.
    /// HWND_TOPMOST : 최상위 위치를 유지(비활성와 되어도)
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="cx">넓이</param>
    /// <param name="cy">높이</param>
    /// <param name="uFlags">플래그</param>
    /// SWP_SHOWWINDOW : 윈도우 표시(이동 크기변경 무시)
    /// SWP_HIDEWINDOW : 윈도우를 숨김(이동 크기변경 무시)
    /// SWP_DRAWFRAME : 윈도우 주변에 프레임을 그림
    /// SWP_NOACTIVATE : 크기 변경 후 윈도우를 활성화 시키지 않음
    /// SWP_NOMOVE : 현재위치 유지
    /// SWP_NOSIZE : 현재크기 유지
    /// SWP_NOZORDER : 현재 Z순서를 그대로 유지
    /// <returns></returns>
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(
        System.IntPtr hWnd, //window handle
        System.IntPtr hWndInsertAfter, // window 배치 순서
        short X, // x position
        short Y, // y position
        short cx, // window width
        short cy, // window height
        uint uFlags // window flags.
    );

    [DllImport("user32.dll")]
    static extern IntPtr SetLayeredWindowAttributes(IntPtr hWnd, int crKey, byte bAlpha, uint dwFlags);

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }


    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    const int GWL_EXSTYLE = -20;

    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TRANSPARENT = 0x00000020;
    const uint LWA_COLORKEY = 0x00000001;

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);


    void Start()
    {
        /*AppWindowUtility.FullScreen = true;
        AppWindowUtility.Transparent = true;
        AppWindowUtility.AlwaysOnTop = true;*/

        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = new Color(0, 0, 0, 0);

#if !UNITY_EDITOR
        IntPtr hWnd = GetActiveWindow();
        MARGINS margins = new MARGINS { cxLeftWidth = -1 };

        DwmExtendFrameIntoClientArea(hWnd, ref margins);

        SetWindowLong(hWnd, GWL_EXSTYLE, WS_EX_LAYERED);
        SetLayeredWindowAttributes(hWnd, 0, 0, LWA_COLORKEY);

        SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
#endif

        Screen.fullScreen = true;
        //AppWindowUtility.AlwaysOnTop = true;
    }
}
