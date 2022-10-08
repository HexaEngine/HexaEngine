print("Hello World!");

function Update()
    var = node.Transform.RotationX;
    var = var + 10 * TimeDelta;
    if var > 360 then var = 0 end
    node.Transform.RotationX = var;
end
