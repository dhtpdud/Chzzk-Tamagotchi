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
    /// window ũ��� ��ġ����
    /// </summary>
    /// <param name="hWnd">������ window handle</param>
    /// <param name="hWndInsertAfter"> Z������ ������ handle �տ� �� handle</param>
    /// HWND_BOTTOM : Z������ �� �Ʒ��� �����츦 ���´�.
    /// HWND_NOTOPMOST : �� �����ִ� ��� ������ �ڿ� �����츦 ���´�.
    /// HWND_TOP : Z������ �� ���� �����츦 ���´�.
    /// HWND_TOPMOST : �ֻ��� ��ġ�� ����(��Ȱ���� �Ǿ)
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="cx">����</param>
    /// <param name="cy">����</param>
    /// <param name="uFlags">�÷���</param>
    /// SWP_SHOWWINDOW : ������ ǥ��(�̵� ũ�⺯�� ����)
    /// SWP_HIDEWINDOW : �����츦 ����(�̵� ũ�⺯�� ����)
    /// SWP_DRAWFRAME : ������ �ֺ��� �������� �׸�
    /// SWP_NOACTIVATE : ũ�� ���� �� �����츦 Ȱ��ȭ ��Ű�� ����
    /// SWP_NOMOVE : ������ġ ����
    /// SWP_NOSIZE : ����ũ�� ����
    /// SWP_NOZORDER : ���� Z������ �״�� ����
    /// <returns></returns>
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(
        System.IntPtr hWnd, //window handle
        System.IntPtr hWndInsertAfter, // window ��ġ ����
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
