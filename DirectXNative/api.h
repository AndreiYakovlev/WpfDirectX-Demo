#pragma once

#include <Windows.h>

#define DIRECTXAPI extern "C" __declspec(dllexport)

DIRECTXAPI LPVOID CreateHwnd();
