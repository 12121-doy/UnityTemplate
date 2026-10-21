Shader "Guisha/BuiltIn/Unlit" // 内置管线的无光照材质，同时保留贴图和颜色。
{ // 开始 Shader 定义。
    Properties // 声明跨管线转换器可以写入的材质属性。
    { // 开始属性列表。
        _MainTex ("主纹理", 2D) = "white" {} // 默认白贴图，缺少纹理时仍能显示基础颜色。
        _Color ("基础颜色", Color) = (1, 1, 1, 1) // 与主贴图相乘的颜色和透明度。
        _Cutoff ("裁剪阈值", Range(0, 1)) = 0.5 // Alpha Test 使用的阈值。
        [HideInInspector] _Surface ("表面类型", Float) = 0 // 保存透明表面标记，供再次切换管线时读取。
        [HideInInspector] _SrcBlend ("源混合", Float) = 1 // 不透明默认采用 One。
        [HideInInspector] _DstBlend ("目标混合", Float) = 0 // 不透明默认采用 Zero。
        [HideInInspector] _ZWrite ("深度写入", Float) = 1 // 默认写入深度；透明转换时可关闭。
    } // 结束属性列表。
    SubShader // 定义内置管线的绘制实现。
    { // 开始子 Shader。
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" } // 默认按普通几何体绘制，转换器可覆盖队列。
        Blend [_SrcBlend] [_DstBlend] // 使用材质控制的混合因子。
        ZWrite [_ZWrite] // 使用材质控制的深度写入开关。
        Pass // 只需一个无光照绘制 Pass。
        { // 开始绘制 Pass。
            CGPROGRAM // 开始顶点与片元程序。
            #pragma vertex vert // 指定顶点入口。
            #pragma fragment frag // 指定片元入口。
            #pragma shader_feature_local _ALPHATEST_ON // 仅在材质需要时启用透明裁剪变体。
            #include "UnityCG.cginc" // 引入内置管线坐标和纹理变换工具。
            sampler2D _MainTex; // 接收主纹理采样器。
            float4 _MainTex_ST; // 接收贴图平铺比例与偏移。
            fixed4 _Color; // 接收基础颜色。
            float _Cutoff; // 接收裁剪阈值。
            struct Input { float4 vertex : POSITION; float2 uv : TEXCOORD0; }; // 顶点输入包含局部坐标与纹理坐标。
            struct Interpolator { float4 position : SV_POSITION; float2 uv : TEXCOORD0; }; // 顶点输出向光栅化阶段传递裁剪坐标和 UV。
            Interpolator vert(Input input) // 将模型顶点与 UV 转换成绘制需要的数据。
            { // 开始顶点程序。
                Interpolator output; // 创建顶点阶段输出。
                output.position = UnityObjectToClipPos(input.vertex); // 把模型局部坐标转换成裁剪空间坐标。
                output.uv = TRANSFORM_TEX(input.uv, _MainTex); // 应用贴图的平铺与偏移。
                return output; // 把转换结果传给光栅化阶段。
            } // 结束顶点程序。
            fixed4 frag(Interpolator input) : SV_Target // 输出当前像素的无光照颜色。
            { // 开始片元程序。
                fixed4 color = tex2D(_MainTex, input.uv) * _Color; // 主纹理乘以基础颜色和透明度。
                #if defined(_ALPHATEST_ON) // 只有启用裁剪关键字的材质才执行。
                clip(color.a - _Cutoff); // 丢弃透明度低于阈值的像素。
                #endif // 结束透明裁剪变体。
                return color; // 输出最终颜色。
            } // 结束片元程序。
            ENDCG // 结束顶点与片元程序。
        } // 结束绘制 Pass。
    } // 结束子 Shader。
} // 结束 Shader 定义。
