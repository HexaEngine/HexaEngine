[maxvertexcount(6)]
void GS_Edges(triangle GS_INPUT input[3], uint faceID : SV_PrimitiveID, inout LineStream edges)
{
    PS_INPUT output = (PS_INPUT) 0;
    output.Info[1] = faceID;
    output.Info[3] = 123; // output 3 edges v0-v1, v1-v2, v2-v1 
    // edge 0 
    output.Info[2] = 0; 
    output.Pos = input[0].Pos; 
    output.Color = input[0].Color; 
    output.Tex = input[0].Tex; 
    edges.Append(output); 
    output.Pos = input[1].Pos; 
    output.Color = input[1].Color; 
    output.Tex = input[1].Tex; 
    edges.Append(output); // edge 1 
    output.Info[2] = 1; 
    output.Pos = input[1].Pos; 
    output.Color = input[0].Color; 
    output.Tex = input[1].Tex; 
    edges.Append(output); 
    output.Pos = input[2].Pos; 
    output.Color = input[2].Color; 
    output.Tex = input[2].Tex; 
    edges.Append(output); // edge 2 
    output.Info[2] = 2; 
    output.Pos = input[2].Pos; 
    output.Color = input[2].Color; 
    output.Tex = input[2].Tex; 
    edges.Append(output); 
    output.Pos = input[0].Pos; 
    output.Color = input[0].Color; 
    output.Tex = input[0].Tex; 
    edges.Append(output); 
    edges.RestartStrip();
    }