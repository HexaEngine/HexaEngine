# cimnodes

This is a c-api wrapper for imnodes (https://github.com/Nelarius/imnodes)  which is a node editor for Dear ImGui (https://github.com/ocornut/imgui)

imnodes is a submodule of this repo. To do generation again in case imnodes is changed, you should update imnodes, make sure that cimgui repository is a sibling folder to this repository folder and execute generator/generator.bat(.sh)

Compilation will need IMNODES_NAMESPACE to be defined as imnodes. With cmake this is done as add_definitions(-DIMNODES_NAMESPACE=imnodes)


