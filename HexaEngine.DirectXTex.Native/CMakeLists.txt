set(TEXTURE_SOURCES  
    HexaTexture.h
    HexaTexture.cpp
    DirectXTex/DirectXTex.h
)

if (IOS OR EMSCRIPTEN)
    add_library(HexaEngine.DirectXTex.Native STATIC ${TEXTURE_SOURCES})
else()
    add_library(HexaEngine.DirectXTex.Native SHARED ${TEXTURE_SOURCES})
endif ()

target_link_libraries(HexaEngine.DirectXTex.Native PUBLIC DirectXTex)
