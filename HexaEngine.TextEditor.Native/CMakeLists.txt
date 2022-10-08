set(TEXTURE_SOURCES  
    TextEditor/TextEditor.h
    TextEditor/TextEditor.cpp
    HexaEditor.h
    HexaEditor.cpp
    ImGui/cimgui.h
)

if (IOS OR EMSCRIPTEN)
    add_library(HexaEngine.TextEditor.Native STATIC ${TEXTURE_SOURCES})
else()
    add_library(HexaEngine.TextEditor.Native SHARED ${TEXTURE_SOURCES})
endif ()

target_link_libraries(HexaEngine.TextEditor.Native PUBLIC imguid)
