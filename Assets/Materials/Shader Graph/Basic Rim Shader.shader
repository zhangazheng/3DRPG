// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Basic Rim Shader"
{
	//-----------------------------------������ || Properties��------------------------------------------  
	Properties
	{
		//����ɫ || Main Color
		_MainColor("Main Color", Color) = (0.5,0.5,0.5,1)
		//���������� || Diffuse Texture
		_TextureDiffuse("Texture Diffuse", 2D) = "white" {}
		//��Ե������ɫ || Rim Color
		_RimColor("Rim Color", Color) = (0.5,0.5,0.5,1)
		//��Ե����ǿ�� ||Rim Power
		_RimPower("Rim Power", Range(0.0, 36)) = 0.1
		//��Ե����ǿ��ϵ�� || Rim Intensity Factor
		_RimIntensity("Rim Intensity", Range(0.0, 100)) = 3
	}

		//----------------------------------������ɫ�� || SubShader��---------------------------------------  
		SubShader
	{
		//��Ⱦ����ΪOpaque����͸�� || RenderType Opaque
		Tags
		{
			"RenderType" = "Opaque"
		}

		//---------------------------------------��Ψһ��ͨ�� || Pass��------------------------------------
		Pass
		{
			//�趨ͨ������ || Set Pass Name
			Name "ForwardBase"

			//���ù���ģʽ || LightMode ForwardBase
			Tags
			{
			}

		//-------------------------����CG��ɫ��������Զ� || Begin CG Programming Part----------------------  
		CGPROGRAM

		//��1��ָ�������Ƭ����ɫ�������� || Set the name of vertex and fragment shader function
		#pragma vertex vert
		#pragma fragment frag

		//��2��ͷ�ļ����� || include
		#include "UnityCG.cginc"
		#include "AutoLight.cginc"

		//��3��ָ��Shader Model 3.0 || Set Shader Model 3.0
		#pragma target 3.0

		//��4���������� || Variable Declaration
		//ϵͳ������ɫ
		uniform float4 _LightColor0;
	//����ɫ
	uniform float4 _MainColor;
	//����������
	uniform sampler2D _TextureDiffuse;
	//����������_ST��׺��
	uniform float4 _TextureDiffuse_ST;
	//��Ե����ɫ
	uniform float4 _RimColor;
	//��Ե��ǿ��
	uniform float _RimPower;
	//��Ե��ǿ��ϵ��
	uniform float _RimIntensity;

	//��5����������ṹ�� || Vertex Input Struct
	struct VertexInput
	{
		//����λ�� || Vertex position
		float4 vertex : POSITION;
		//������������ || Normal vector coordinates
		float3 normal : NORMAL;
		//һ���������� || Primary texture coordinates
		float4 texcoord : TEXCOORD0;
	};

	//��6����������ṹ�� || Vertex Output Struct
	struct VertexOutput
	{
		//����λ�� || Pixel position
		float4 pos : SV_POSITION;
		//һ���������� || Primary texture coordinates
		float4 texcoord : TEXCOORD0;
		//������������ || Normal vector coordinates
		float3 normal : NORMAL;
		//����ռ��е�����λ�� || Coordinate position in world space
		float4 posWorld : TEXCOORD1;
		//������Դ����,�������õĹ��� || Function in AutoLight.cginc to create light coordinates
		LIGHTING_COORDS(3,4)
	};

	//��7��������ɫ���� || Vertex Shader Function
	VertexOutput vert(VertexInput v)
	{
		//��1������һ����������ṹ���� || Declares a vertex output structure object
		VertexOutput o;

		//��2����������ṹ || Fill the output structure
		//�������������긳ֵ�������������
		o.texcoord = v.texcoord;
		//��ȡ����������ռ��еķ�����������  
		o.normal = mul(float4(v.normal,0), unity_WorldToObject).xyz;
		//��ö���������ռ��е�λ������  
		o.posWorld = mul(unity_ObjectToWorld, v.vertex);
		//��ȡ����λ��
		o.pos = UnityObjectToClipPos(v.vertex);

		//��3�����ش�����ṹ����  || Returns the output structure
		return o;
	}

	//��8��Ƭ����ɫ���� || Fragment Shader Function
	fixed4 frag(VertexOutput i) : COLOR
	{
		//��8.1���������׼�� || Direction
		//�ӽǷ���
		float3 ViewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
		//���߷���
		float3 Normalection = normalize(i.normal);
		//���շ���
		float3 LightDirection = normalize(_WorldSpaceLightPos0.xyz);

		//��8.2��������յ�˥�� || Lighting attenuation
		//˥��ֵ
		float Attenuation = LIGHT_ATTENUATION(i);
		//˥������ɫֵ
		float3 AttenColor = Attenuation * _LightColor0.xyz;

		//��8.3������������ || Diffuse
		float NdotL = dot(Normalection, LightDirection);
		float3 Diffuse = max(0.0, NdotL) * AttenColor + UNITY_LIGHTMODEL_AMBIENT.xyz;

		//��8.4��׼���Է������ || Emissive
		//�����Եǿ��
		half Rim = 1.0 - max(0, dot(i.normal, ViewDirection));
		//�������Ե�Է���ǿ��
		float3 Emissive = _RimColor.rgb * pow(Rim,_RimPower) * _RimIntensity;

		//��8.5������������ɫ�м����Է�����ɫ || Calculate the final color
		//������ɫ = ��������ϵ�� x ������ɫ x rgb��ɫ��+�Է�����ɫ || Final Color=(Diffuse x Texture x rgbColor)+Emissive
		float3 finalColor = Diffuse * (tex2D(_TextureDiffuse,TRANSFORM_TEX(i.texcoord.rg, _TextureDiffuse)).rgb * _MainColor.rgb) + Emissive;

		//��8.6������������ɫ || Return final color
		return fixed4(finalColor,1);
	}

		//-------------------����CG��ɫ��������Զ� || End CG Programming Part------------------  
		ENDCG
	}
	}

		//����ɫ��Ϊ��ͨ������ || Fallback use Diffuse
		FallBack "Diffuse"
}