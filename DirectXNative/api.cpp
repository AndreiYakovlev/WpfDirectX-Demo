#include "api.h"

LRESULT WINAPI MsgProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	return DefWindowProc(hWnd, msg, wParam, lParam);
}

DIRECTXAPI LPVOID CreateHwnd() {
	WNDCLASS wndclass;

	LPTSTR className = TEXT("Foo");
	wndclass.style = CS_HREDRAW | CS_VREDRAW;
	wndclass.lpfnWndProc = DefWindowProc;
	wndclass.cbClsExtra = 0;
	wndclass.cbWndExtra = 0;
	wndclass.hInstance = NULL;
	wndclass.hIcon = LoadIcon(NULL, IDI_APPLICATION);
	wndclass.hCursor = LoadCursor(NULL, IDC_ARROW);
	wndclass.hbrBackground = (HBRUSH)GetStockObject(WHITE_BRUSH);
	wndclass.lpszMenuName = NULL;
	wndclass.lpszClassName = className;

	// create the device's window
	RegisterClass(&wndclass);
	HWND hWnd = CreateWindow(
		className, TEXT("D3DImageSample"),
		WS_OVERLAPPEDWINDOW, //Стиль окна
		0, 0, 0, 0, NULL, NULL, NULL, NULL);

	return hWnd;
}