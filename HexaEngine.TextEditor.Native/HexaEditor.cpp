#include "HexaEditor.h"

TextEditor *CreateTextEditor()
{
    return new TextEditor();
}

void ReleaseTextEditor(TextEditor **editor)
{
    free(*editor);
    *editor = nullptr;
}

void Render(TextEditor *editor, const char *aTitle, const ImVec2 &aSize, bool aBorder)
{
    editor->Render(aTitle, aSize, aBorder);
}