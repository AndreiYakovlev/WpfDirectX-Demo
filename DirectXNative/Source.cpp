#include <iostream>
#include <cstdio>
using namespace std;

#include "scene.h"

int main() {

	
	void* p = InitializeScene();

	ReleaseScene();

	return 0;
}