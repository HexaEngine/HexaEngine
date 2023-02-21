#include "imgui.h"
#include "imgui_internal.h"
#include "./imnodes/imnodes.h"
#include "cimnodes.h"



#include "auto_funcs.cpp"

//needed for io.link_detach_with_modifier_click.modifier = &ImGui::GetIO().KeyCtrl
CIMGUI_API bool* getIOKeyCtrlPtr()
{
    return &ImGui::GetIO().KeyCtrl;
}
