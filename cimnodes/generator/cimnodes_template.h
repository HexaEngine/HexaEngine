#ifndef CIMNODES_INCLUDED
#define CIMNODES_INCLUDED

#include "cimgui.h"

#ifdef CIMGUI_DEFINE_ENUMS_AND_STRUCTS
#include "imgui_structs.h"
#else
#endif // CIMGUI_DEFINE_ENUMS_AND_STRUCTS

#include "auto_funcs.h"

//needed for io.link_detach_with_modifier_click.modifier = &ImGui::GetIO().KeyCtrl
CIMGUI_API bool* getIOKeyCtrlPtr();

#endif //CIMNODES_INCLUDED




