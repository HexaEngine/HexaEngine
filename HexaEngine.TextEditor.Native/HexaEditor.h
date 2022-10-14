#include "TextEditor/TextEditor.h"

#define API __declspec(dllexport)

extern "C"
{
    API TextEditor *CreateTextEditor();
    API void ReleaseTextEditor(TextEditor **editor);
    API void Render(TextEditor *editor, const char *aTitle, const ImVec2 &aSize = ImVec2(), bool aBorder = false);
}