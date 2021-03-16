using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Hyperkill;
using UnityEngine;
// ReSharper disable UnusedMember.Global

namespace Hyperkill
{
	public class Hax : MonoBehaviour
	{
		private MethodInfo fn;
		private bool       patched;

		public void Start()
		{
			//Do stuff here once for initialization
			Debug.Log("hi");


			/*var il=fn.GetMethodBody().GetILAsByteArray();
 
			 for (int i = 0; i < il.Length; i++) {
				 byte b = il[i];
 
				 if (b==OpCodes.Ldc_I4_S.Value&&il[i+1]==0x10) {
 
					 il[i + 1] = 100;
					 patched   = true;
					 break;
				 }
			 }
 
			 InjectionHelper.UpdateILCodes(fn, il);*/


		}

		public void jit()
		{
			fn = typeof(Shotgun).GetMethod("Shoot",
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			
			RuntimeHelpers.PrepareMethod(fn.MethodHandle);

			var il=fn.GetMethodBody().GetILAsByteArray();


			// for (int i = 0; i < il.Length; i++) {
			// 	if (il[i]==OpCodes.Ldc_R4.Value) {
			// 		il[3] = 0xa0;
			// 		il[4] = 0x40;
			// 	}
			// }
			//
			// InjectionHelper.UpdateILCodes(fn,il);
		}

		public void Update()
		{
			//Do stuff here on every tic
		}

		public void OnGUI()
		{
			// Make a background box
			GUI.Box(new Rect(10, 10, 100, 90), "Loader Menu");

			// Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
			if (GUI.Button(new Rect(20, 40, 80, 20), $"jit")) {
				jit();
			}

			DrawCrosshair();
		}

		private void DrawCrosshair()
		{

			var cam = FindObjectOfType<Camera>(); //Get camera

			//var cam = Camera.main;
			var target =
				cam.transform.position + cam.transform.forward; //Calculate a point facing straight away from us
			
			var w2s = cam.WorldToScreenPoint(target);           //Translate position to screen

			if (w2s.z < 0) //Behind screen?
				return;    //Skip
			
			ZatsRenderer.DrawBox(new Vector2(w2s.x, w2s.y), new Vector2(1, 1), Color.green);
			
				//ZatsRenderer.DrawString(new Vector2(w2s.x, w2s.y), "BUTT SEX"); //Draw
		}
	}

	public class Loader
	{
		public static void Init()
		{
			Loader.Load = new GameObject();
			Loader.Load.AddComponent<Hax>();
			UnityEngine.Object.DontDestroyOnLoad(Loader.Load);
		}

		public static void Unload()
		{
			UnityEngine.Object.Destroy(Load);
		}

		private static GameObject Load;
	}
	/*
	 * .\smi.exe inject -p ULTRAKILL -a .\bin\Release\Hyperkill.dll -n Hyperkill -c Loader -m Init
	 *
	 * .\smi.exe eject -p ULTRAKILL -a <addr> -n Hyperkill -c Loader -m Unload
	 */
	class Program
	{
		static void Main(string[] args) { }
	}
}