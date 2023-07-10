using COM3D2API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

namespace COM3D2.LiveLink.Plugin
{

	internal class LiveLinkMenu : MonoSingleton<LiveLinkMenu>
	{
		public static int IconWidth => 80;

		private bool buttonAdded = false;
		private bool inPhotoMode = false;

		private static bool displayUI = false;
		private Rect uiWindow = new Rect((float)(Screen.width * 3.5 / 5), 120, 120, 50);

		public static byte[] GearIcon
		{
			get
			{
				if (_gearIcon == null)
				{
					Stream pngStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("COM3D2.LiveLink.Plugin.AnmGearIcon.png");
					pngStream.Read(_gearIcon, 0, (int)pngStream.Length);
				}
				return _gearIcon;
			}
		}
		static byte[] _gearIcon = null;

		private static string pathAnm = UTY.gameProjectPath + "\\Mod\\[AnmMaker]\\[Anm]";
		private static string pathPrj = UTY.gameProjectPath + "\\Mod\\[AnmMaker]\\[Prj]";

		void Awake()
		{
			SceneManager.sceneLoaded += SceneLoaded;
		}

		void SceneLoaded(Scene s, LoadSceneMode lsm)
		{
			inPhotoMode = false;

			//Add the button
			if (GameMain.Instance != null && s != null && s.name.Equals("SceneTitle") && GameMain.Instance.SysShortcut != null && !buttonAdded)
			{
				SystemShortcutAPI.AddButton("LiveLink", OnMenuButtonClickCallback, "Live Link", GearIcon);
				buttonAdded = true;
			}

			//Special boolean for displaying info message in edit mode
			if (GameMain.Instance != null && s.name.Equals("ScenePhotoMode"))
			{
				inPhotoMode = true;
				displayUI = false;
			}
		}

		private void OnMenuButtonClickCallback()
		{
			if (!System.IO.Directory.Exists(pathAnm))
			{
				System.IO.Directory.CreateDirectory(pathAnm);
			}
			if (!System.IO.Directory.Exists(pathPrj))
			{
				System.IO.Directory.CreateDirectory(pathPrj);
			}

			//infoMessage = "";

			if (true || inPhotoMode)
			{
				//Toggle UI
				displayUI = !displayUI;
			}
			else
			{
				displayUI = false;
			}
		}

		void OnGUI()
		{
			if (displayUI)
			{
				uiWindow = GUILayout.Window(416807, uiWindow, DisplayUIWindow, "Animation Maker", GUILayout.Width((IconWidth * 5) + 35), GUILayout.Height(Screen.height * 2 / 3 - 40));
			}
		}

		private void DisplayUIWindow(int windowId)
		{
			Pages.Page page = Pages.Page.AllPages[Pages.Page.CurrentPage];

			// Make it draggable. This must always be last.
			GUI.DragWindow();
		}
	}

}


namespace COM3D2.LiveLink.Plugin.Pages
{
	internal abstract class Page
	{
		protected static int iconWidth = 80;

		protected static string infoMessage = "";

		protected abstract string HeaderText { get; }
		protected Vector2 Scroll;

		public static int CurrentPage { get; protected set; } = 0;

		public static Page[] AllPages = new Page[] {
			new SelectPage(),
			new ManPage(),
			new Page2(),
			new Page3(),
			new Page4(),
			new MaidPage(),
			new Page6(), //new ProjFilesPage(),
			new TimelinePage()
		};
		
		public virtual void OnGUI()
		{
			GUILayout.BeginVertical("box");
			{
				GUILayout.Label("INFO:");
				GUILayout.Label(infoMessage);

				GUILayout.Label("Animation Type");

				OnPageGUI();
			}
			GUILayout.EndVertical();
		}

		protected void DisplayPage(int pageNumber)
		{
			CurrentPage = pageNumber;
		}

		protected virtual void BeginScrollView()
		{
			Scroll = GUILayout.BeginScrollView(Scroll, false, true, GUILayout.Width((iconWidth * 5) + 20), GUILayout.Height(Screen.height * 2 / 3 - 60));
		}

		protected virtual void EndScrollView()
		{
			GUILayout.EndScrollView();
		}

		protected abstract void OnPageGUI();
	}

	internal class BlankPage : Page
	{
		protected sealed override string HeaderText => "";
		public sealed override void OnGUI() { }
		protected sealed override void BeginScrollView() { }
		protected sealed override void EndScrollView() { }
		protected sealed override void OnPageGUI() { }

	}

	// Page 0
	internal class SelectPage : Page
	{
		protected override string HeaderText => "Animation Type";

		private static Maid baseMaidMan;
		
		protected override void OnPageGUI()
		{
			GUILayout.Label("INFO:");
			GUILayout.Label(infoMessage);

			GUILayout.Label("Animation Type");

			if (GUILayout.Button("XTMS Transfer"))
			{
				CurrentPage = 1;
			}
			if (GUILayout.Button("Custom Creator"))
			{
				CurrentPage = 5;
			}
		}
	}

	// Page 1
	internal class ManPage : Page
	{
		protected override string HeaderText => "Man Link";

		private static Maid baseMaidMan;

		protected override void OnPageGUI()
		{
			GUILayout.Label("Select Linked Man");
			BeginScrollView();
			{
				// Loop available men
				for (int i = 0; i < GameMain.Instance.CharacterMgr.GetManCount(); i++)
				{
					Maid nextMaid = GameMain.Instance.CharacterMgr.GetMan(i);
					if (nextMaid != null && nextMaid.isActiveAndEnabled)
					{
						//Draw a button
						if (GUILayout.Button(new GUIContent(nextMaid.GetThumIcon()), GUILayout.Width(iconWidth), GUILayout.Height(iconWidth)))
						{
							baseMaidMan = nextMaid;
							CurrentPage = 5;
							//newAnm = new Structs.NewAnm();
						}
					}
				}
			}
			EndScrollView();

			if (GUILayout.Button("Back"))
			{
				baseMaidMan = null;
				CurrentPage = 0;
			}
		}
	}

	// Page 2
	internal class Page2 : BlankPage { }

	// Page 3
	internal class Page3 : BlankPage { }

	// Page 4
	internal class Page4 : BlankPage { }

	// Page 5
	internal class MaidPage : Page
	{
		protected override string HeaderText => "Select Maid to Record";

		private static Dictionary<MPN, Dictionary<string, float>> recordMaidBaseSks;
		private object baseMaidMan = null;

		protected override void OnPageGUI()
		{
			BeginScrollView();
			{
				//Loop available maids
				for (int i = 0; i < GameMain.Instance.CharacterMgr.GetMaidCount(); i++)
				{
					Maid nextMaid = GameMain.Instance.CharacterMgr.GetMaid(i);
					if (nextMaid != null)
					{
						//string[] maidNameSplit = nextMaid.Parts.name.Split(']');
						//string maidName = maidNameSplit[maidNameSplit.Length - 1];

						//Draw a button
						if (GUILayout.Button(new GUIContent(nextMaid.GetThumIcon()), GUILayout.Width(iconWidth), GUILayout.Height(iconWidth)))
						{
							LiveLinkRecorder.RecordingMaid = nextMaid;
							CurrentPage = 6;
							//projFiles = new List<String>();
							//projFiles.AddRange(Directory.GetFiles(pathPrj));
							//foreach (string anmFilePathTemp in Directory.GetFiles(pathAnm))
							//{
							//	PhotoMotionData.AddMyPose(anmFilePathTemp);
							//}
							//newAnm = new NewAnm();
						}
					}
				}
			}
			EndScrollView();

			if (GUILayout.Button("Back"))
			{
				LiveLinkRecorder.RecordingMaid = null;
				CurrentPage = 0;

				if (baseMaidMan != null)
				{
					baseMaidMan = null;
					CurrentPage = 1;
				}
			}
		}
	}

	// Page 6
	/*
	internal class ProjFilesPage : Page
	{
		protected override string HeaderText => "Project Files";

		private static List<String> projFiles = new List<string>();

		private static string anmAnimationSearch;
		private static string anmAnimationFilter;
		private static Vector2 anmAnimationMotionsScroll;
		private static string anmAnimationSelectedName;
		private static string anmAnimationTimeText = "0.00";
		private static string timeStepText = "0";
		private static bool timeStepPressed = false;
		private static List<float> autoFrames = new List<float>();
		private static NewAnm newAnm;
		private static string anmFile = "";

		private static bool autoPoseNext => LiveLinkRecorder.autoPoseNext;
		private static bool autoSnapNext => LiveLinkRecorder.autoSnapNext;
		private static bool captureBones => LiveLinkRecorder.captureBones;
		private static bool captureSks   => LiveLinkRecorder.captureSks  ;

		private Vector2 page6_scroll_frames;

		public override void OnPageGUI()
		{
			GUILayout.Label("Load:");
			BeginScrollView();
			{
				for (int i = 0; i < projFiles.Count; i++)
				{
					string nextProjFilePath = projFiles[i];
					string[] splitProjFilePath = nextProjFilePath.Split('\\');
					if (GUILayout.Button(splitProjFilePath[splitProjFilePath.Length - 1]))
					{
						newAnm = Newtonsoft.Json.JsonConvert.DeserializeObject<NewAnm>(System.IO.File.ReadAllText(nextProjFilePath));
						anmFile = Path.Combine(pathAnm, (newAnm.name) + ".anm");
					}
				}
			}
			EndScrollView();

			GUILayout.Label("Name:");
			newAnm.name = GUILayout.TextField(newAnm.name);

			// Man
			if (baseMaidMan != null)
			{
				GUILayout.BeginVertical();
				{
					GUILayout.Label("Man:");
					UIHelper.UI_5_DrawMotionControls(baseMaidMan, ref anmAnimationSearch, ref anmAnimationFilter, ref anmAnimationMotionsScroll, ref anmAnimationSelectedName, ref anmAnimationTimeText, ref timeStepText, ref timeStepPressed, ref autoFrames, ref autoPoseNext, ref newAnm, ref infoMessage, iconWidth);
				}
				GUILayout.EndVertical();
			}

			// Maid
			if (baseMaidMan == null && recordingMaid != null)
			{
				GUILayout.BeginVertical();
				{
					GUILayout.Label("Maid:");
					UIHelper.UI_5_DrawMotionControls(recordingMaid, ref anmAnimationSearch, ref anmAnimationFilter, ref anmAnimationMotionsScroll, ref anmAnimationSelectedName, ref anmAnimationTimeText, ref timeStepText, ref timeStepPressed, ref autoFrames, ref autoPoseNext, ref newAnm, ref infoMessage, iconWidth);
				}
				GUILayout.EndVertical();
			}

			GUILayout.Label("Capture:");
			GUILayout.BeginHorizontal();
			{
				//schedule_icon_gravure.tex
				if (GUILayout.Button("Bones")) //|| autoSnapNow)
				{
					captureBones = true;
				}
				if (GUILayout.Button("ShapeKeys"))
				{
					captureSks = true;
				}
				if (GUILayout.Button("All"))
				{
					captureBones = true;
					captureSks = true;
				}
			}
			page6_scroll_frames = GUILayout.BeginScrollView(page6_scroll_frames, false, true, GUILayout.Width((iconWidth * 5) + 20), GUILayout.Height(Screen.height * 1 / 8));
			{
				//Loop the frames made
				int frameIndex = -1;
				int frameButtonPress = -1;

				//Sort the frames first
				newAnm.frames.Sort(delegate (NewAnmFrame x, NewAnmFrame y)
				{
					return x.frameTime.CompareTo(y.frameTime);
				});
				for (int i = 0; i < newAnm.frames.Count; i++)
				{
					NewAnmFrame nextFrame = newAnm.frames[i];

					GUILayout.BeginHorizontal();
					{
						if (GUILayout.Button(nextFrame.timestamp + "\n" + nextFrame.frameTime) && frameButtonPress == -1)
						{
							//Drilldown into this frame
							frameIndex = i;
							frameButtonPress = 0;
						}
						GUI.enabled = (i != 0);
						if (GUILayout.Button("↑") && frameButtonPress == -1)
						{
							//Move the frame up
							frameIndex = i;
							frameButtonPress = 1;
						}
						GUI.enabled = (i != (newAnm.frames.Count - 1));
						if (GUILayout.Button("↓") && frameButtonPress == -1)
						{
							//Move the frame down
							frameIndex = i;
							frameButtonPress = 2;
						}
						GUI.enabled = true;
						if (GUILayout.Button("X") && frameButtonPress == -1)
						{
							//Delete the frame
							frameIndex = i;
							frameButtonPress = 3;
						}
					}
					GUILayout.EndHorizontal();
				}

				//Perform after the loop to avoid issue with concurrancy
				switch (frameButtonPress)
				{
					case 0:
						{
							//Drilldown into this frame
							newAnmCurrentFrame = newAnm.frames[frameIndex];

							//Pause animations
							Animation anim = LiveLinkRecorder.RecordingMaid.GetAnimation();
							if (anim != null)
							{
								foreach (AnimationState anmState in anim)
								{
									anmState.enabled = false;
								}
							}

							//Loop available maids
							//for (int i = 0; i < GameMain.Instance.CharacterMgr.GetMaidCount(); i++)
							//{
							//    Maid nextMaid = GameMain.Instance.CharacterMgr.GetMaid(i);
							//    if (nextMaid != null)
							//    {
							//        Animation anim = nextMaid.GetAnimation();
							//        if (anim != null)
							//        {
							//            foreach (AnimationState anmState in anim)
							//            {
							//                anmState.enabled = false;
							//            }
							//        }
							//    }
							//}
							////Loop available men
							//for (int i = 0; i < GameMain.Instance.CharacterMgr.GetManCount(); i++)
							//{
							//    Maid nextMaid = GameMain.Instance.CharacterMgr.GetMan(i);
							//    if (nextMaid != null && nextMaid.isActiveAndEnabled)
							//    {
							//        Animation anim = nextMaid.GetAnimation();
							//        if (anim != null)
							//        {
							//            foreach (AnimationState anmState in anim)
							//            {
							//                anmState.enabled = false;
							//            }
							//        }
							//    }
							//}

							//Load the current frame into the Maid
							newAnmCurrentFrame.PlaybackBones(new CacheBoneDataArray.BoneData(LiveLinkRecorder.RecordingMaid.body0.GetBone("Bip01"), null), LiveLinkRecorder.RecordingMaid);
							CurrentPage = 7;
							break;
						}
					case 1:
						{
							//Move the frame up
							float swapTime = newAnm.frames[frameIndex].frameTime;
							newAnm.frames[frameIndex].frameTime = newAnm.frames[frameIndex - 1].frameTime;
							newAnm.frames[frameIndex - 1].frameTime = swapTime;
							break;
						}
					case 2:
						{
							//Move the frame down (more like move next frame up)
							float swapTime = newAnm.frames[frameIndex + 1].frameTime;
							newAnm.frames[frameIndex + 1].frameTime = newAnm.frames[frameIndex].frameTime;
							newAnm.frames[frameIndex].frameTime = swapTime;
							break;
						}
					case 3:
						{
							//Delete the frame
							newAnm.frames.RemoveAt(frameIndex);
							break;
						}
				}
			}
			GUILayout.EndScrollView();

			if (GUILayout.Button("Save Project"))
			{
				string newPath = Path.Combine(pathPrj, (newAnm.name + ".json"));
				File.WriteAllText(newPath, Newtonsoft.Json.JsonConvert.SerializeObject(newAnm));
			}
			
			if (GUILayout.Button("Export .anm"))
			{
				if (newAnm.WriteToBinary())
				{
					anmFile = Path.Combine(pathAnm, (newAnm.name) + ".anm");
				}
			}
			{
				GUI.enabled = File.Exists(anmFile);
				if (GUILayout.Button("Test .anm"))
				{
					if (LiveLinkRecorder.RecordingMaid != null)
					{
						foreach (PhotoMotionData motionData in PhotoMotionData.data)
						{
							if (motionData.direct_file.Equals(anmFile))
							{
								motionData.Apply(LiveLinkRecorder.RecordingMaid);
							}
						}
						//LiveLinkRecorder.RecordingMaid.CrossFade(anmFile,, false, true, false, 0f, 1f);
					}
				}
				GUI.enabled = true;
			}

			if (GUILayout.Button("Back"))
			{
				anmAnimationSelectedName = null;
				anmAnimationTimeText = "0.00";
				timeStepText = "0";
				autoFrames = new List<float>();
				newAnm = null;
				anmFile = "";
				CurrentPage = 5;
			}
		}
	}
	*/
	internal class Page6 : BlankPage { }

	// Page 7
	internal class TimelinePage : Page
	{
		protected override string HeaderText => "Timeline";

		private static Structs.NewAnmFrame newAnmCurrentFrame = null;
		private static string frameTimeText = null;
		private static float frameTimeCached = -1f;

		protected override void OnPageGUI()
		{
			GUILayout.Label("Name:");
			newAnmCurrentFrame.timestamp = GUILayout.TextField(newAnmCurrentFrame.timestamp);

			GUILayout.Label("Frame Time:");
			GUILayout.BeginHorizontal();
			{
				//I made this stupid complicated
				if (frameTimeText == null)
				{
					frameTimeText = newAnmCurrentFrame.frameTime.ToString();
				}
				if (frameTimeCached == -1)
				{
					frameTimeCached = newAnmCurrentFrame.frameTime;
				}

				frameTimeText = GUILayout.TextField(frameTimeText);

				if (GUILayout.Button("Apply"))
				{
					if (System.Text.RegularExpressions.Regex.IsMatch(frameTimeText, @"[\+]?\d*\.?\d*") && float.Parse(frameTimeText) >= 0)
					{
						newAnmCurrentFrame.frameTime = float.Parse(frameTimeText);
					}
					else
					{
						infoMessage = "Invalid Frame Time float";
					}
				}
				if (GUILayout.Button("Original"))
				{
					newAnmCurrentFrame.frameTime = frameTimeCached;
					frameTimeText = null;
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Label("Re-Position/Rotate:");
			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("SNAP"))
				{
					//Record the Maid's data
					newAnmCurrentFrame.RecordBones(new CacheBoneDataArray.BoneData(LiveLinkRecorder.RecordingMaid.body0.GetBone("Bip01"), null));
				}
			}
			GUILayout.EndHorizontal();

			if (GUILayout.Button("Back"))
			{
				newAnmCurrentFrame = null;
				frameTimeText = null;
				CurrentPage = 6;
			}
		}
	}
}
