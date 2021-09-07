﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HelixToolkit.Wpf;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.IO;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Forms.Design;
using System.Collections;
using System.Numerics;
using System.Data.Common;
using System.Data.SqlClient;
using System.Windows;
using System.Xml.Serialization;
using System.Reflection;

namespace MK7_KMP_Editor_For_PG_
{
    public partial class Form1 : Form
    {
        //UserControl1.xamlの初期化
        //ここは作成時の名前にも影響されるので必ず確認すること
        public UserControl1 render = new UserControl1();

        KMPs.KMPHelper.FlagConverter.EnemyRoute EnemyRouteFlagConverter = new KMPs.KMPHelper.FlagConverter.EnemyRoute();
        KMPs.KMPHelper.FlagConverter.GlideRoute GlideRouteFlagConverter = new KMPs.KMPHelper.FlagConverter.GlideRoute();

        #region MV3DList
        KMPs.KMPViewportObject KMPViewportObject = new KMPs.KMPViewportObject
        {
            Area_MV3DList = new List<ModelVisual3D>(),
            Camera_MV3DList = new List<ModelVisual3D>(),
            EnemyRoute_Rail_List = new List<HTK_3DES.PathTools.Rail>(),
            ItemRoute_Rail_List = new List<HTK_3DES.PathTools.Rail>(),
            GlideRoute_Rail_List = new List<HTK_3DES.PathTools.Rail>(),
            Routes_List = new List<HTK_3DES.PathTools.Rail>(),
            Checkpoint_Rail = new List<HTK_3DES.KMP_3DCheckpointSystem.Checkpoint>(),
            OBJ_MV3DList = new List<ModelVisual3D>(),
            RespawnPoint_MV3DList = new List<ModelVisual3D>(),
            StartPosition_MV3DList = new List<ModelVisual3D>()
        };

        //Course Object
        public Dictionary<string, ArrayList> MV3D_Dictionary = new Dictionary<string, ArrayList>();
        #endregion

        public KMPPropertyGridSettings.TPTK_Section TPTK_Section { get; set; }
        public KMPPropertyGridSettings.HPNE_TPNE_Section HPNE_TPNE_Section { get; set; }
        public KMPPropertyGridSettings.HPTI_TPTI_Section HPTI_TPTI_Section { get; set; }
        public KMPPropertyGridSettings.HPKC_TPKC_Section HPKC_TPKC_Section { get; set; }
        public KMPPropertyGridSettings.JBOG_section JBOG_Section { get; set; }
        public KMPPropertyGridSettings.ITOP_Section ITOP_Section { get; set; }
        public KMPPropertyGridSettings.AERA_Section AERA_Section { get; set; }
        public KMPPropertyGridSettings.EMAC_Section EMAC_Section { get; set; }
        public KMPPropertyGridSettings.TPGJ_Section TPGJ_Section { get; set; }
        public KMPPropertyGridSettings.IGTS_Section IGTS_Section { get; set; }
        public KMPPropertyGridSettings.HPLG_TPLG_Section HPLG_TPLG_Section { get; set; }


        public Form1()
        {
            InitializeComponent();

            render.MainViewPort.CalculateCursorPosition = true;

            render.MouseLeftButtonDown += Render_MouseLeftButtonDown;
            render.MouseMove += Render_MouseMove;

            //ElementHost1のKey
            KeyPreview = true;
            elementHost1.Child = render;
            elementHost1.Child.KeyDown += render_KeyDown;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AddKMPSection.Anchor = AnchorStyles.Bottom;
            DeleteKMPSection.Anchor = AnchorStyles.Bottom;

            KMP_Group_ListBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Top;
            propertyGrid_KMP_Group.Anchor = AnchorStyles.Bottom | AnchorStyles.Top;

            KMP_Path_ListBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Top;
            propertyGrid_KMP_Path.Anchor = AnchorStyles.Bottom | AnchorStyles.Top;

            propertyGrid_KMP_StageInfo.Anchor = AnchorStyles.Bottom | AnchorStyles.Top;

            elementHost1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            tabControl1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            KMP_Viewport_SplitContainer.FixedPanel = FixedPanel.Panel2;
            KMP_Viewport_SplitContainer.Panel2Collapsed = true;
            KMP_Viewport_SplitContainer.Panel2.Hide();

            //Panelの固定
            KMP_Main_SplitContainer.FixedPanel = FixedPanel.Panel1;
            KMP_Main_SplitContainer.IsSplitterFixed = true;

            writeBinaryToolStripMenuItem.Enabled = false;
            closeKMPToolStripMenuItem.Enabled = false;
            exportToolStripMenuItem.Enabled = false;
            closeObjToolStripMenuItem.Enabled = false;
            visibilityToolStripMenuItem.Enabled = false;
            xXXXRouteExporterToolStripMenuItem.Enabled = false;
            xXXXRouteImporterToolStripMenuItem.Enabled = false;
            inputXmlAsXXXXToolStripMenuItem.Enabled = false;

            string CD = System.IO.Directory.GetCurrentDirectory();
            if (Directory.Exists(CD + "\\KMP_OBJ") == false)
            {
                var KMPOBJ = Properties.Resources.KMP_OBJ;
                FileStream fs1 = new FileStream("KMP_OBJ.zip", FileMode.Create, FileAccess.Write);
                fs1.Write(KMPOBJ, 0, KMPOBJ.Length);
                fs1.Close();
                fs1.Dispose();
                System.IO.Compression.ZipFile.ExtractToDirectory("KMP_OBJ.zip", CD);
            }

            if (Directory.Exists(CD + "\\KMPObjectFlow") == false)
            {
                var ObjFlowModel = Properties.Resources.KMPObjectFlow;
                FileStream fs1 = new FileStream("KMPObjectFlow.zip", FileMode.Create, FileAccess.Write);
                fs1.Write(ObjFlowModel, 0, ObjFlowModel.Length);
                fs1.Close();
                fs1.Dispose();
                System.IO.Compression.ZipFile.ExtractToDirectory("KMPObjectFlow.zip", CD);
            }
        }

        #region Import OBJ
        private void readObjToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog2 = new OpenFileDialog()
            {
                Title = "Open Model",
                InitialDirectory = @"C:\Users\User\Desktop",
                Filter = "obj file|*.obj"
            };

            if (openFileDialog2.ShowDialog() != DialogResult.OK) return;

            Model3DGroup M3D_Group = null;
            ObjReader OBJ_Reader = new ObjReader();
            M3D_Group = OBJ_Reader.Read(openFileDialog2.FileName);

            for (int n = 0; n < M3D_Group.Children.Count; n++)
            {
                Model3D NewM3D = M3D_Group.Children[n];
                ModelVisual3D MV3D = new ModelVisual3D
                {
                    Content = NewM3D
                };

                GeometryModel3D GM3D = (GeometryModel3D)M3D_Group.Children[n];
                string MatName = GM3D.Material.GetName();

                //ModelVisual3Dに名前をつける
                MV3D.SetName(MatName + " -1 -1");

                ArrayList arrayList = new ArrayList();
                arrayList.Add(false);
                arrayList.Add(MV3D);

                try
                {
                    MV3D_Dictionary.Add(MatName, arrayList);
                }
                catch (System.ArgumentException)
                {
                    //マテリアルの名前が同じだった場合
                    MV3D_Dictionary.Add(MatName + n, arrayList);
                }

                //表示
                render.MainViewPort.Children.Add(MV3D);
            }

            closeObjToolStripMenuItem.Enabled = true;
            visibilityToolStripMenuItem.Enabled = true;
        }

        private void visibilityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ModelVisibilityForm modelVisibilityForm = new ModelVisibilityForm();
            modelVisibilityForm.Show();
        }

        private void closeObjToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Close Course Model
            foreach(var f in MV3D_Dictionary)
            {
                render.MainViewPort.Children.Remove((ModelVisual3D)f.Value[1]);
            }

            MV3D_Dictionary.Clear();

            closeObjToolStripMenuItem.Enabled = false;
            visibilityToolStripMenuItem.Enabled = false;
        }
        #endregion

        HitTestResult HTR = null;
        ModelVisual3D FindMV3D = null;
        HTK_3DES.TSRSystem.Transform_Value transform_Value = null;

        private void Render_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //MainViewPortのマウス座標を取得
            var Vp_MousePos2D = e.GetPosition(this.render);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) == true)
            {
                //マウス座標からヒットテストを実行
                HitTestResult HTRs = VisualTreeHelper.HitTest(render.MainViewPort, Vp_MousePos2D);
                HTR = HTRs as RayMeshGeometry3DHitTestResult;
                if (HTR != null)
                {
                    string[] MDLStr_GetName = null;
                    if (typeof(ModelVisual3D) == HTR.VisualHit.GetType())
                    {
                        //ダウンキャスト
                        FindMV3D = (ModelVisual3D)HTR.VisualHit;
                        MDLStr_GetName = HTR.VisualHit.GetName().Split(' ');
                    }
                    if (typeof(LinesVisual3D) == HTR.VisualHit.GetType()) return;
                    if (typeof(TubeVisual3D) == HTR.VisualHit.GetType()) return;

                    ////ダウンキャスト
                    //FindMV3D = (ModelVisual3D)HTR.VisualHit;
                    //string[] MDLStr_GetName = HTR.VisualHit.GetName().Split(' ');

                    #region Get Object info
                    string OBJ_Name = MDLStr_GetName[0];
                    int MDLNum = int.Parse(MDLStr_GetName[1]);
                    int GroupNum = int.Parse(MDLStr_GetName[2]);
                    #endregion

                    if (OBJ_Name == "StartPosition")
                    {
                        //Dictionaryに存在するKeyを検索
                        if (KMPViewportObject.StartPosition_MV3DList.Contains(FindMV3D))
                        {
                            #region SelectedIndex
                            KMPSectionComboBox.SelectedIndex = KMPSectionComboBox.Items.IndexOf("KartPoint");
                            KMP_Path_ListBox.SelectedIndex = MDLNum;
                            tabControl1.SelectedIndex = 1;
                            #endregion

                            transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                            {
                                Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                                {
                                    X = TPTK_Section.TPTKValueList[MDLNum].Rotate_Value.X,
                                    Y = TPTK_Section.TPTKValueList[MDLNum].Rotate_Value.Y,
                                    Z = TPTK_Section.TPTKValueList[MDLNum].Rotate_Value.Z
                                },
                                Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                                {
                                    X = 10,
                                    Y = 10,
                                    Z = 10
                                },
                                Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                                {
                                    X = TPTK_Section.TPTKValueList[MDLNum].Position_Value.X,
                                    Y = TPTK_Section.TPTKValueList[MDLNum].Position_Value.Y,
                                    Z = TPTK_Section.TPTKValueList[MDLNum].Position_Value.Z
                                }
                            };

                            //出力
                            Section_Name_LBL.Text = OBJ_Name;
                            Section_ID_LBL.Text = MDLNum.ToString();
                            Section_Group_ID_LBL.Text = GroupNum.ToString();
                        }
                    }
                    if (OBJ_Name == "EnemyRoute")
                    {
                        //Dictionaryに存在するKeyを検索
                        if (KMPViewportObject.EnemyRoute_Rail_List[GroupNum].MV3D_List.Contains(FindMV3D))
                        {
                            #region SelectedIndex
                            KMPSectionComboBox.SelectedIndex = KMPSectionComboBox.Items.IndexOf("EnemyRoutes");
                            KMP_Group_ListBox.SelectedIndex = GroupNum;
                            KMP_Path_ListBox.SelectedIndex = MDLNum;
                            tabControl1.SelectedIndex = 1;
                            #endregion

                            transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                            {
                                Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                                {
                                    X = 0,
                                    Y = 0,
                                    Z = 0
                                },
                                Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                                {
                                    X = HPNE_TPNE_Section.HPNEValueList[GroupNum].TPNEValueList[MDLNum].Control * 100,
                                    Y = HPNE_TPNE_Section.HPNEValueList[GroupNum].TPNEValueList[MDLNum].Control * 100,
                                    Z = HPNE_TPNE_Section.HPNEValueList[GroupNum].TPNEValueList[MDLNum].Control * 100
                                },
                                Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                                {
                                    X = HPNE_TPNE_Section.HPNEValueList[GroupNum].TPNEValueList[MDLNum].Positions.X,
                                    Y = HPNE_TPNE_Section.HPNEValueList[GroupNum].TPNEValueList[MDLNum].Positions.Y,
                                    Z = HPNE_TPNE_Section.HPNEValueList[GroupNum].TPNEValueList[MDLNum].Positions.Z
                                }
                            };

                            //出力
                            Section_Name_LBL.Text = OBJ_Name;
                            Section_ID_LBL.Text = MDLNum.ToString();
                            Section_Group_ID_LBL.Text = GroupNum.ToString();
                        }
                    }
                    if (OBJ_Name == "ItemRoute")
                    {
                        if (KMPViewportObject.ItemRoute_Rail_List[GroupNum].MV3D_List.Contains(FindMV3D))
                        {
                            #region SelectedIndex
                            KMPSectionComboBox.SelectedIndex = KMPSectionComboBox.Items.IndexOf("ItemRoutes");
                            KMP_Group_ListBox.SelectedIndex = GroupNum;
                            KMP_Path_ListBox.SelectedIndex = MDLNum;
                            tabControl1.SelectedIndex = 1;
                            #endregion

                            transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                            {
                                Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                                {
                                    X = 0,
                                    Y = 0,
                                    Z = 0
                                },
                                Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                                {
                                    X = HPTI_TPTI_Section.HPTIValueList[GroupNum].TPTIValueList[MDLNum].TPTI_PointSize * 100,
                                    Y = HPTI_TPTI_Section.HPTIValueList[GroupNum].TPTIValueList[MDLNum].TPTI_PointSize * 100,
                                    Z = HPTI_TPTI_Section.HPTIValueList[GroupNum].TPTIValueList[MDLNum].TPTI_PointSize * 100
                                },
                                Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                                {
                                    X = HPTI_TPTI_Section.HPTIValueList[GroupNum].TPTIValueList[MDLNum].TPTI_Positions.X,
                                    Y = HPTI_TPTI_Section.HPTIValueList[GroupNum].TPTIValueList[MDLNum].TPTI_Positions.Y,
                                    Z = HPTI_TPTI_Section.HPTIValueList[GroupNum].TPTIValueList[MDLNum].TPTI_Positions.Z
                                }
                            };

                            //出力
                            Section_Name_LBL.Text = OBJ_Name;
                            Section_ID_LBL.Text = MDLNum.ToString();
                            Section_Group_ID_LBL.Text = GroupNum.ToString();
                        }
                    }
                    if (OBJ_Name == "Checkpoint_Left")
                    {
                        if (KMPViewportObject.Checkpoint_Rail[GroupNum].Checkpoint_Left.MV3D_List.Contains(FindMV3D))
                        {
                            #region SelectedIndex
                            KMPSectionComboBox.SelectedIndex = KMPSectionComboBox.Items.IndexOf("CheckPoint");
                            KMP_Group_ListBox.SelectedIndex = GroupNum;
                            KMP_Path_ListBox.SelectedIndex = MDLNum;
                            tabControl1.SelectedIndex = 1;
                            #endregion

                            transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                            {
                                Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                                {
                                    X = 0,
                                    Y = 0,
                                    Z = 0
                                },
                                Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                                {
                                    X = 50,
                                    Y = 50,
                                    Z = 50
                                },
                                Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                                {
                                    X = HPKC_TPKC_Section.HPKCValueList[GroupNum].TPKCValueList[MDLNum].Position_2D_Left.X,
                                    Y = Convert.ToDouble(textBox1.Text),
                                    Z = HPKC_TPKC_Section.HPKCValueList[GroupNum].TPKCValueList[MDLNum].Position_2D_Left.Y
                                }
                            };

                            //出力
                            Section_Name_LBL.Text = OBJ_Name;
                            Section_ID_LBL.Text = MDLNum.ToString();
                            Section_Group_ID_LBL.Text = GroupNum.ToString();
                        }
                    }
                    if (OBJ_Name == "Checkpoint_Right")
                    {
                        if (KMPViewportObject.Checkpoint_Rail[GroupNum].Checkpoint_Right.MV3D_List.Contains(FindMV3D))
                        {
                            #region SelectedIndex
                            KMPSectionComboBox.SelectedIndex = KMPSectionComboBox.Items.IndexOf("CheckPoint");
                            KMP_Group_ListBox.SelectedIndex = GroupNum;
                            KMP_Path_ListBox.SelectedIndex = MDLNum;
                            tabControl1.SelectedIndex = 1;
                            #endregion

                            transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                            {
                                Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                                {
                                    X = 0,
                                    Y = 0,
                                    Z = 0
                                },
                                Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                                {
                                    X = 50,
                                    Y = 50,
                                    Z = 50
                                },
                                Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                                {
                                    X = HPKC_TPKC_Section.HPKCValueList[GroupNum].TPKCValueList[MDLNum].Position_2D_Right.X,
                                    Y = Convert.ToDouble(textBox1.Text),
                                    Z = HPKC_TPKC_Section.HPKCValueList[GroupNum].TPKCValueList[MDLNum].Position_2D_Right.Y
                                }
                            };

                            //出力
                            Section_Name_LBL.Text = OBJ_Name;
                            Section_ID_LBL.Text = MDLNum.ToString();
                            Section_Group_ID_LBL.Text = GroupNum.ToString();
                        }
                    }
                    if (OBJ_Name == "OBJ")
                    {
                        if (KMPViewportObject.OBJ_MV3DList.Contains(FindMV3D))
                        {
                            #region SelectedIndex
                            KMPSectionComboBox.SelectedIndex = KMPSectionComboBox.Items.IndexOf("Obj");
                            KMP_Path_ListBox.SelectedIndex = MDLNum;
                            tabControl1.SelectedIndex = 1;
                            #endregion

                            transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                            {
                                Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                                {
                                    X = JBOG_Section.JBOGValueList[MDLNum].Rotations.X,
                                    Y = JBOG_Section.JBOGValueList[MDLNum].Rotations.Y,
                                    Z = JBOG_Section.JBOGValueList[MDLNum].Rotations.Z
                                },
                                Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                                {
                                    X = JBOG_Section.JBOGValueList[MDLNum].Scales.X * 2,
                                    Y = JBOG_Section.JBOGValueList[MDLNum].Scales.Y * 2,
                                    Z = JBOG_Section.JBOGValueList[MDLNum].Scales.Z * 2
                                },
                                Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                                {
                                    X = JBOG_Section.JBOGValueList[MDLNum].Positions.X,
                                    Y = JBOG_Section.JBOGValueList[MDLNum].Positions.Y,
                                    Z = JBOG_Section.JBOGValueList[MDLNum].Positions.Z
                                }
                            };

                            //出力
                            Section_Name_LBL.Text = OBJ_Name;
                            Section_ID_LBL.Text = MDLNum.ToString();
                            Section_Group_ID_LBL.Text = GroupNum.ToString();
                        }
                    }
                    if (OBJ_Name == "Routes")
                    {
                        if (KMPViewportObject.Routes_List[GroupNum].MV3D_List.Contains(FindMV3D))
                        {
                            #region SelectedIndex
                            KMPSectionComboBox.SelectedIndex = KMPSectionComboBox.Items.IndexOf("Route");
                            KMP_Group_ListBox.SelectedIndex = GroupNum;
                            KMP_Path_ListBox.SelectedIndex = MDLNum;
                            tabControl1.SelectedIndex = 1;
                            #endregion

                            transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                            {
                                Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                                {
                                    X = 0,
                                    Y = 0,
                                    Z = 0
                                },
                                Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                                {
                                    X = 10,
                                    Y = 10,
                                    Z = 10
                                },
                                Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                                {
                                    X = ITOP_Section.ITOP_RouteList[GroupNum].ITOP_PointList[MDLNum].Positions.X,
                                    Y = ITOP_Section.ITOP_RouteList[GroupNum].ITOP_PointList[MDLNum].Positions.Y,
                                    Z = ITOP_Section.ITOP_RouteList[GroupNum].ITOP_PointList[MDLNum].Positions.Z
                                }
                            };

                            //出力
                            Section_Name_LBL.Text = OBJ_Name;
                            Section_ID_LBL.Text = MDLNum.ToString();
                            Section_Group_ID_LBL.Text = GroupNum.ToString();
                        }
                    }
                    if (OBJ_Name == "Area")
                    {
                        if (KMPViewportObject.Area_MV3DList.Contains(FindMV3D))
                        {
                            #region SelectedIndex
                            KMPSectionComboBox.SelectedIndex = KMPSectionComboBox.Items.IndexOf("Area");
                            KMP_Path_ListBox.SelectedIndex = MDLNum;
                            tabControl1.SelectedIndex = 1;
                            #endregion

                            transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                            {
                                Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                                {
                                    X = AERA_Section.AERAValueList[MDLNum].Rotations.X,
                                    Y = AERA_Section.AERAValueList[MDLNum].Rotations.Y,
                                    Z = AERA_Section.AERAValueList[MDLNum].Rotations.Z
                                },
                                Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                                {
                                    X = AERA_Section.AERAValueList[MDLNum].Scales.X * 1000,
                                    Y = AERA_Section.AERAValueList[MDLNum].Scales.Y * 1000,
                                    Z = AERA_Section.AERAValueList[MDLNum].Scales.Z * 1000
                                },
                                Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                                {
                                    X = AERA_Section.AERAValueList[MDLNum].Positions.X,
                                    Y = AERA_Section.AERAValueList[MDLNum].Positions.Y,
                                    Z = AERA_Section.AERAValueList[MDLNum].Positions.Z
                                }
                            };

                            //出力
                            Section_Name_LBL.Text = OBJ_Name;
                            Section_ID_LBL.Text = MDLNum.ToString();
                            Section_Group_ID_LBL.Text = GroupNum.ToString();
                        }
                    }
                    if (OBJ_Name == "Camera")
                    {
                        if (KMPViewportObject.Camera_MV3DList.Contains(FindMV3D))
                        {
                            #region SelectedIndex
                            KMPSectionComboBox.SelectedIndex = KMPSectionComboBox.Items.IndexOf("Camera");
                            KMP_Path_ListBox.SelectedIndex = MDLNum;
                            tabControl1.SelectedIndex = 1;
                            #endregion

                            transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                            {
                                Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                                {
                                    X = EMAC_Section.EMACValueList[MDLNum].Rotations.X,
                                    Y = EMAC_Section.EMACValueList[MDLNum].Rotations.Y,
                                    Z = EMAC_Section.EMACValueList[MDLNum].Rotations.Z
                                },
                                Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                                {
                                    X = 10,
                                    Y = 10,
                                    Z = 10
                                },
                                Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                                {
                                    X = EMAC_Section.EMACValueList[MDLNum].Positions.X,
                                    Y = EMAC_Section.EMACValueList[MDLNum].Positions.Y,
                                    Z = EMAC_Section.EMACValueList[MDLNum].Positions.Z
                                }
                            };

                            //出力
                            Section_Name_LBL.Text = OBJ_Name;
                            Section_ID_LBL.Text = MDLNum.ToString();
                            Section_Group_ID_LBL.Text = GroupNum.ToString();
                        }
                    }
                    if (OBJ_Name == "RespawnPoint")
                    {
                        if (KMPViewportObject.RespawnPoint_MV3DList.Contains(FindMV3D))
                        {
                            #region SelectedIndex
                            KMPSectionComboBox.SelectedIndex = KMPSectionComboBox.Items.IndexOf("JugemPoint");
                            KMP_Path_ListBox.SelectedIndex = MDLNum;
                            tabControl1.SelectedIndex = 1;
                            #endregion

                            transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                            {
                                Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                                {
                                    X = TPGJ_Section.TPGJValueList[MDLNum].Rotations.X,
                                    Y = TPGJ_Section.TPGJValueList[MDLNum].Rotations.Y,
                                    Z = TPGJ_Section.TPGJValueList[MDLNum].Rotations.Z
                                },
                                Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                                {
                                    X = 10,
                                    Y = 10,
                                    Z = 10
                                },
                                Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                                {
                                    X = TPGJ_Section.TPGJValueList[MDLNum].Positions.X,
                                    Y = TPGJ_Section.TPGJValueList[MDLNum].Positions.Y,
                                    Z = TPGJ_Section.TPGJValueList[MDLNum].Positions.Z
                                }
                            };

                            //出力
                            Section_Name_LBL.Text = OBJ_Name;
                            Section_ID_LBL.Text = MDLNum.ToString();
                            Section_Group_ID_LBL.Text = GroupNum.ToString();
                        }
                    }
                    if (OBJ_Name == "GlideRoutes")
                    {
                        if (KMPViewportObject.GlideRoute_Rail_List[GroupNum].MV3D_List.Contains(FindMV3D))
                        {
                            #region SelectedIndex
                            KMPSectionComboBox.SelectedIndex = KMPSectionComboBox.Items.IndexOf("GlideRoutes");
                            KMP_Group_ListBox.SelectedIndex = GroupNum;
                            KMP_Path_ListBox.SelectedIndex = MDLNum;
                            tabControl1.SelectedIndex = 1;
                            #endregion

                            transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                            {
                                Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                                {
                                    X = 0,
                                    Y = 0,
                                    Z = 0
                                },
                                Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                                {
                                    X = HPLG_TPLG_Section.HPLGValueList[GroupNum].TPLGValueList[MDLNum].TPLG_PointScaleValue * 100,
                                    Y = HPLG_TPLG_Section.HPLGValueList[GroupNum].TPLGValueList[MDLNum].TPLG_PointScaleValue * 100,
                                    Z = HPLG_TPLG_Section.HPLGValueList[GroupNum].TPLGValueList[MDLNum].TPLG_PointScaleValue * 100
                                },
                                Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                                {
                                    X = HPLG_TPLG_Section.HPLGValueList[GroupNum].TPLGValueList[MDLNum].Positions.X,
                                    Y = HPLG_TPLG_Section.HPLGValueList[GroupNum].TPLGValueList[MDLNum].Positions.Y,
                                    Z = HPLG_TPLG_Section.HPLGValueList[GroupNum].TPLGValueList[MDLNum].Positions.Z
                                }
                            };

                            //出力
                            Section_Name_LBL.Text = OBJ_Name;
                            Section_ID_LBL.Text = MDLNum.ToString();
                            Section_Group_ID_LBL.Text = GroupNum.ToString();
                        }
                    }
                    if (MV3D_Dictionary.ContainsKey(OBJ_Name))
                    {
                        //出力
                        Section_Name_LBL.Text = MDLStr_GetName[0];
                        Section_ID_LBL.Text = MDLStr_GetName[1] + ": None";
                        Section_Group_ID_LBL.Text = MDLStr_GetName[2] + ": None";
                    }
                }
                if (HTR == null)
                {
                    Section_Name_LBL.Text = "Not Selected.";
                    Section_ID_LBL.Text = "<!>Null<!>";
                    Section_Group_ID_LBL.Text = "<!>Null<!>";
                }
            }
            if (Keyboard.IsKeyDown(Key.LeftAlt) == true)
            {
                Point3D Pos = new Point3D();
                if (ViewportTypeChange.Checked == true)
                {
                    Pos = render.ViewportPosition(UserControl1.PositionMode.ElementPosition);
                }
                if (ViewportTypeChange.Checked == false)
                {
                    Pos = render.ViewportPosition(UserControl1.PositionMode.MouseCursor);
                }

                if (KMPSectionComboBox.Text == "KartPoint")
                {
                    KMPPropertyGridSettings.TPTK_Section.TPTKValue tPTKValue = new KMPPropertyGridSettings.TPTK_Section.TPTKValue
                    {
                        ID = KMP_Path_ListBox.Items.Count,
                        Player_Index = 0,
                        Position_Value = new KMPPropertyGridSettings.TPTK_Section.TPTKValue.Position
                        {
                            X = (float)Pos.X,
                            Y = (float)Pos.Y,
                            Z = (float)Pos.Z
                        },
                        Rotate_Value = new KMPPropertyGridSettings.TPTK_Section.TPTKValue.Rotation
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        },
                        TPTK_UnkBytes = 0
                    };

                    TPTK_Section.TPTKValueList.Add(tPTKValue);

                    KMP_Path_ListBox.Items.Add(tPTKValue);

                    #region Add Model(StartPosition)
                    HTK_3DES.TSRSystem.Transform_Value StartPosition_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                    {
                        Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                        {
                            X = Convert.ToDouble(tPTKValue.Position_Value.X),
                            Y = Convert.ToDouble(tPTKValue.Position_Value.Y),
                            Z = Convert.ToDouble(tPTKValue.Position_Value.Z)
                        },
                        Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                        {
                            X = 10,
                            Y = 10,
                            Z = 10
                        },
                        Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                        {
                            X = Convert.ToDouble(tPTKValue.Rotate_Value.X),
                            Y = Convert.ToDouble(tPTKValue.Rotate_Value.Y),
                            Z = Convert.ToDouble(tPTKValue.Rotate_Value.Z)
                        }
                    };

                    ModelVisual3D dv3D_StartPositionOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\StartPosition\\StartPosition.obj");

                    //モデルの名前と番号を文字列に格納(情報化)
                    HTK_3DES.TSRSystem.SetString_MV3D(dv3D_StartPositionOBJ, "StartPosition " + tPTKValue.ID + " " + -1);

                    HTK_3DES.TransformMV3D.Transform_MV3D(StartPosition_transform_Value, dv3D_StartPositionOBJ);

                    KMPViewportObject.StartPosition_MV3DList.Add(dv3D_StartPositionOBJ);

                    render.MainViewPort.Children.Add(dv3D_StartPositionOBJ);

                    HTK_3DES.TSRSystem.GC_Dispose(dv3D_StartPositionOBJ);
                    #endregion
                }
                if (KMPSectionComboBox.Text == "EnemyRoutes")
                {
                    if (KMP_Group_ListBox.SelectedIndex != -1)
                    {
                        KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue tPNEValue = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue
                        {
                            Group_ID = KMP_Group_ListBox.SelectedIndex,
                            ID = KMP_Path_ListBox.Items.Count,
                            Positions = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.Position
                            {
                                X = (float)Pos.X,
                                Y = (float)Pos.Y,
                                Z = (float)Pos.Z
                            },
                            Control = 1,
                            MushSettings = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.MushSetting
                            {
                                MushSettingValue = 0
                            },
                            DriftSettings = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.DriftSetting
                            {
                                DriftSettingValue = 0
                            },
                            FlagSettings = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.FlagSetting
                            {
                                Flags = 0
                            },
                            PathFindOptions = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.PathFindOption
                            {
                                PathFindOptionValue = 0
                            },
                            MaxSearchYOffset = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.MaxSearch_YOffset
                            {
                                MaxSearchYOffsetValue = 0
                            }
                        };

                        HPNE_TPNE_Section.HPNEValueList[KMP_Group_ListBox.SelectedIndex].TPNEValueList.Add(tPNEValue);

                        KMP_Path_ListBox.Items.Add(tPNEValue);

                        #region Add Model(EnemyRoutes)
                        HTK_3DES.TSRSystem.Transform_Value EnemyPoint_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                        {
                            Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                            {
                                X = Convert.ToDouble(tPNEValue.Positions.X),
                                Y = Convert.ToDouble(tPNEValue.Positions.Y),
                                Z = Convert.ToDouble(tPNEValue.Positions.Z)
                            },
                            Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                            {
                                X = tPNEValue.Control * 100,
                                Y = tPNEValue.Control * 100,
                                Z = tPNEValue.Control * 100
                            },
                            Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                            {
                                X = 0,
                                Y = 0,
                                Z = 0
                            }
                        };

                        ModelVisual3D dv3D_EnemyPathOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\EnemyPath\\EnemyPath.obj");

                        //モデルの名前と番号を文字列に格納(情報化)
                        HTK_3DES.TSRSystem.SetString_MV3D(dv3D_EnemyPathOBJ, "EnemyRoute " + tPNEValue.ID + " " + tPNEValue.Group_ID);

                        HTK_3DES.TransformMV3D.Transform_MV3D(EnemyPoint_transform_Value, dv3D_EnemyPathOBJ);

                        //Add Rail => MV3DList
                        KMPViewportObject.EnemyRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].MV3D_List.Add(dv3D_EnemyPathOBJ);

                        render.MainViewPort.Children.Add(dv3D_EnemyPathOBJ);
                        #endregion

                        HTK_3DES.PathTools.ResetRail(render, KMPViewportObject.EnemyRoute_Rail_List[KMP_Group_ListBox.SelectedIndex], HTK_3DES.PathTools.RailType.Tube);
                        List<Point3D> point3Ds = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.EnemyRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].MV3D_List);
                        KMPViewportObject.EnemyRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].TV3D_List = HTK_3DES.PathTools.DrawPath_Tube(render, point3Ds, 10.0, Colors.Orange);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Group : The group may not be selected or it may be empty.");
                    }
                }
                if (KMPSectionComboBox.Text == "ItemRoutes")
                {
                    if (KMP_Group_ListBox.SelectedIndex != -1)
                    {
                        KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue tPTIValue = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue
                        {
                            Group_ID = KMP_Group_ListBox.SelectedIndex,
                            ID = KMP_Path_ListBox.Items.Count,
                            TPTI_Positions = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue.TPTI_Position
                            {
                                X = (float)Pos.X,
                                Y = (float)Pos.Y,
                                Z = (float)Pos.Z
                            },
                            TPTI_PointSize = 1,
                            GravityModeSettings = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue.GravityModeSetting
                            {
                                GravityModeValue = 0
                            },
                            PlayerScanRadiusSettings = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue.PlayerScanRadiusSetting
                            {
                                PlayerScanRadiusValue = 0
                            }
                        };

                        HPTI_TPTI_Section.HPTIValueList[KMP_Group_ListBox.SelectedIndex].TPTIValueList.Add(tPTIValue);

                        KMP_Path_ListBox.Items.Add(tPTIValue);

                        #region Add Model(ItemRoutes)
                        HTK_3DES.TSRSystem.Transform_Value ItemPoint_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                        {
                            Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                            {
                                X = Convert.ToDouble(tPTIValue.TPTI_Positions.X),
                                Y = Convert.ToDouble(tPTIValue.TPTI_Positions.Y),
                                Z = Convert.ToDouble(tPTIValue.TPTI_Positions.Z)
                            },
                            Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                            {
                                X = Convert.ToDouble(tPTIValue.TPTI_PointSize) * 100,
                                Y = Convert.ToDouble(tPTIValue.TPTI_PointSize) * 100,
                                Z = Convert.ToDouble(tPTIValue.TPTI_PointSize) * 100
                            },
                            Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                            {
                                X = 0,
                                Y = 0,
                                Z = 0
                            }
                        };

                        ModelVisual3D dv3D_ItemPathOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\ItemPath\\ItemPath.obj");

                        //モデルの名前と番号を文字列に格納(情報化)
                        HTK_3DES.TSRSystem.SetString_MV3D(dv3D_ItemPathOBJ, "ItemRoute " + tPTIValue.ID + " " + tPTIValue.Group_ID);

                        HTK_3DES.TransformMV3D.Transform_MV3D(ItemPoint_transform_Value, dv3D_ItemPathOBJ);

                        //Add Rail => MV3DList
                        KMPViewportObject.ItemRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].MV3D_List.Add(dv3D_ItemPathOBJ);

                        render.MainViewPort.Children.Add(dv3D_ItemPathOBJ);
                        #endregion

                        HTK_3DES.PathTools.ResetRail(render, KMPViewportObject.ItemRoute_Rail_List[KMP_Group_ListBox.SelectedIndex], HTK_3DES.PathTools.RailType.Tube);
                        List<Point3D> point3Ds = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.ItemRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].MV3D_List);
                        KMPViewportObject.ItemRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].TV3D_List = HTK_3DES.PathTools.DrawPath_Tube(render, point3Ds, 10.0, Colors.Green);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Group : The group may not be selected or it may be empty.");
                    }
                }
                if (KMPSectionComboBox.Text == "CheckPoint")
                {
                    if (KMP_Group_ListBox.SelectedIndex != -1)
                    {
                        KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue tPKCValue = new KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue
                        {
                            Group_ID = KMP_Group_ListBox.SelectedIndex,
                            ID = KMP_Path_ListBox.Items.Count,
                            Position_2D_Left = new KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue.Position2D_Left
                            {
                                X = (float)Pos.X,
                                Y = (float)Pos.Z
                            },
                            Position_2D_Right = new KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue.Position2D_Right
                            {
                                X = (float)Pos.X,
                                Y = (float)Pos.Z
                            },
                            TPKC_RespawnID = 0xFF,
                            TPKC_Checkpoint_Type = 0,
                            TPKC_NextCheckPoint = 0xFF,
                            TPKC_PreviousCheckPoint = 0xFF,
                            TPKC_UnkBytes1 = 0,
                            TPKC_UnkBytes2 = 0,
                            TPKC_UnkBytes3 = 0,
                            TPKC_UnkBytes4 = 0
                        };

                        HPKC_TPKC_Section.HPKCValueList[KMP_Group_ListBox.SelectedIndex].TPKCValueList.Add(tPKCValue);

                        KMP_Path_ListBox.Items.Add(tPKCValue);

                        #region Create
                        var P2D_Left = tPKCValue.Position_2D_Left;
                        Vector2 P2DLeftToVector2 = new Vector2(Convert.ToSingle(P2D_Left.X), Convert.ToSingle(P2D_Left.Y));
                        Point3D P3DLeft = KMPs.KMPHelper.Vector3DTo2DConverter.Vector2DTo3D(P2DLeftToVector2, KMPs.KMPHelper.Vector3DTo2DConverter.Axis_Up.Y).ToPoint3D();
                        P3DLeft.Y = Convert.ToDouble(textBox1.Text);

                        #region Transform(Left)
                        HTK_3DES.TSRSystem.Transform_Value P2DLeft_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                        {
                            Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                            {
                                X = P3DLeft.X,
                                Y = P3DLeft.Y,
                                Z = P3DLeft.Z
                            },
                            Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                            {
                                X = 50,
                                Y = 50,
                                Z = 50
                            },
                            Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                            {
                                X = 0,
                                Y = 0,
                                Z = 0
                            }
                        };

                        ModelVisual3D dv3D_CheckpointLeftOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Checkpoint\\LeftPoint\\Checkpoint_Left.obj");

                        //モデルの名前と番号を文字列に格納(情報化)
                        HTK_3DES.TSRSystem.SetString_MV3D(dv3D_CheckpointLeftOBJ, "Checkpoint_Left " + tPKCValue.ID + " " + tPKCValue.Group_ID);

                        HTK_3DES.TransformMV3D.Transform_MV3D(P2DLeft_transform_Value, dv3D_CheckpointLeftOBJ);

                        KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Left.MV3D_List.Add(dv3D_CheckpointLeftOBJ);

                        render.MainViewPort.Children.Add(dv3D_CheckpointLeftOBJ);

                        HTK_3DES.TSRSystem.GC_Dispose(dv3D_CheckpointLeftOBJ);
                        #endregion

                        var P2D_Right = tPKCValue.Position_2D_Right;
                        Vector2 P2DRightToVector2 = new Vector2(Convert.ToSingle(P2D_Right.X), Convert.ToSingle(P2D_Right.Y));
                        Point3D P3DRight = KMPs.KMPHelper.Vector3DTo2DConverter.Vector2DTo3D(P2DRightToVector2, KMPs.KMPHelper.Vector3DTo2DConverter.Axis_Up.Y).ToPoint3D();
                        P3DRight.Y = Convert.ToDouble(textBox1.Text);

                        #region Transform(Right)
                        HTK_3DES.TSRSystem.Transform_Value P2DRight_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                        {
                            Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                            {
                                X = P3DRight.X,
                                Y = P3DRight.Y,
                                Z = P3DRight.Z
                            },
                            Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                            {
                                X = 50,
                                Y = 50,
                                Z = 50
                            },
                            Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                            {
                                X = 0,
                                Y = 0,
                                Z = 0
                            }
                        };

                        ModelVisual3D dv3D_CheckpointRightOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Checkpoint\\RightPoint\\Checkpoint_Right.obj");

                        //モデルの名前と番号を文字列に格納(情報化)
                        HTK_3DES.TSRSystem.SetString_MV3D(dv3D_CheckpointRightOBJ, "Checkpoint_Right " + tPKCValue.ID + " " + tPKCValue.Group_ID);

                        HTK_3DES.TransformMV3D.Transform_MV3D(P2DRight_transform_Value, dv3D_CheckpointRightOBJ);

                        KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Right.MV3D_List.Add(dv3D_CheckpointRightOBJ);

                        render.MainViewPort.Children.Add(dv3D_CheckpointRightOBJ);

                        HTK_3DES.TSRSystem.GC_Dispose(dv3D_CheckpointRightOBJ);
                        #endregion

                        List<Point3D> point3Ds = new List<Point3D>();
                        point3Ds.Add(P3DLeft);
                        point3Ds.Add(P3DRight);

                        LinesVisual3D linesVisual3D = new LinesVisual3D
                        {
                            Points = new Point3DCollection(point3Ds),
                            Thickness = 1,
                            Color = Colors.Black
                        };

                        KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Line.Add(linesVisual3D);
                        render.MainViewPort.Children.Add(linesVisual3D);
                        #endregion

                        HTK_3DES.PathTools.ResetRail(render, KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Left, HTK_3DES.PathTools.RailType.Line);
                        List<Point3D> point3Ds_Left = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Left.MV3D_List);
                        KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Left.LV3D_List = HTK_3DES.PathTools.DrawPath_Line(render, point3Ds_Left, 5, Colors.Green);

                        HTK_3DES.PathTools.ResetRail(render, KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Right, HTK_3DES.PathTools.RailType.Line);
                        List<Point3D> point3Ds_Right = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Right.MV3D_List);
                        KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Right.LV3D_List = HTK_3DES.PathTools.DrawPath_Line(render, point3Ds_Right, 5, Colors.Red);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Group : The group may not be selected or it may be empty.");
                    }
                }
                if (KMPSectionComboBox.Text == "Obj")
                {
                    AddKMPObjectForm addKMPObjectForm = new AddKMPObjectForm();
                    addKMPObjectForm.ShowDialog();

                    var data = addKMPObjectForm.SelectedKMPObject_Info;

                    KMPPropertyGridSettings.JBOG_section.JBOGValue jBOGValue = new KMPPropertyGridSettings.JBOG_section.JBOGValue
                    {
                        ID = KMP_Path_ListBox.Items.Count,
                        ObjectName = data.Name,
                        ObjectID = data.ObjID,
                        JBOG_ITOP_RouteIDIndex = 65535,
                        JBOG_PresenceSetting = 7,
                        JBOG_UnkByte1 = "0000",
                        JBOG_UnkByte2 = "FFFF",
                        JBOG_UnkByte3 = 0,
                        Positions = new KMPPropertyGridSettings.JBOG_section.JBOGValue.Position
                        {
                            X = (float)Pos.X,
                            Y = (float)Pos.Y,
                            Z = (float)Pos.Z
                        },
                        Scales = new KMPPropertyGridSettings.JBOG_section.JBOGValue.Scale
                        {
                            X = 1,
                            Y = 1,
                            Z = 1
                        },
                        Rotations = new KMPPropertyGridSettings.JBOG_section.JBOGValue.Rotation
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        },
                        JOBJ_Specific_Setting = new KMPPropertyGridSettings.JBOG_section.JBOGValue.JBOG_SpecificSetting
                        {
                            Value0 = 0,
                            Value1 = 0,
                            Value2 = 0,
                            Value3 = 0,
                            Value4 = 0,
                            Value5 = 0,
                            Value6 = 0,
                            Value7 = 0
                        }
                    };

                    JBOG_Section.JBOGValueList.Add(jBOGValue);

                    KMP_Path_ListBox.Items.Add(jBOGValue);

                    #region Add Model(OBJ)
                    HTK_3DES.TSRSystem.Transform_Value OBJ_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                    {
                        Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                        {
                            X = Convert.ToDouble(jBOGValue.Positions.X),
                            Y = Convert.ToDouble(jBOGValue.Positions.Y),
                            Z = Convert.ToDouble(jBOGValue.Positions.Z)
                        },
                        Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                        {
                            X = Convert.ToDouble(jBOGValue.Scales.X) * 2,
                            Y = Convert.ToDouble(jBOGValue.Scales.Y) * 2,
                            Z = Convert.ToDouble(jBOGValue.Scales.Z) * 2
                        },
                        Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                        {
                            X = Convert.ToDouble(jBOGValue.Rotations.X),
                            Y = Convert.ToDouble(jBOGValue.Rotations.Y),
                            Z = Convert.ToDouble(jBOGValue.Rotations.Z)
                        }
                    };

                    KMPs.KMPHelper.ObjFlowReader.ObjFlowXmlToObject objFlowXmlToObject = KMPs.KMPHelper.ObjFlowReader.ReadObjFlowXml("ObjFlowData.xml");
                    string Path = objFlowXmlToObject.ObjFlows.Find(x => x.ObjectID == data.ObjID).Path;
                    ModelVisual3D dv3D_OBJ = HTK_3DES.TSRSystem.OBJReader(Path);

                    //モデルの名前と番号を文字列に格納(情報化)
                    HTK_3DES.TSRSystem.SetString_MV3D(dv3D_OBJ, "OBJ " + jBOGValue.ID + " " + -1);

                    HTK_3DES.TransformMV3D.Transform_MV3D(OBJ_transform_Value, dv3D_OBJ);

                    KMPViewportObject.OBJ_MV3DList.Add(dv3D_OBJ);

                    render.MainViewPort.Children.Add(dv3D_OBJ);
                    #endregion
                }
                if (KMPSectionComboBox.Text == "Route")
                {
                    if (KMP_Group_ListBox.SelectedIndex != -1)
                    {
                        KMPPropertyGridSettings.ITOP_Section.ITOP_Route.ITOP_Point iTOP_Point = new KMPPropertyGridSettings.ITOP_Section.ITOP_Route.ITOP_Point
                        {
                            GroupID = KMP_Group_ListBox.SelectedIndex,
                            ID = KMP_Path_ListBox.Items.Count,
                            Positions = new KMPPropertyGridSettings.ITOP_Section.ITOP_Route.ITOP_Point.Position
                            {
                                X = (float)Pos.X,
                                Y = (float)Pos.Y,
                                Z = (float)Pos.Z
                            },
                            ITOP_PointSetting2 = 0,
                            ITOP_Point_RouteSpeed = 0
                        };

                        ITOP_Section.ITOP_RouteList[KMP_Group_ListBox.SelectedIndex].ITOP_PointList.Add(iTOP_Point);

                        KMP_Path_ListBox.Items.Add(iTOP_Point);

                        #region Add Model(Routes)
                        HTK_3DES.TSRSystem.Transform_Value JugemPath_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                        {
                            Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                            {
                                X = Convert.ToSingle(iTOP_Point.Positions.X),
                                Y = Convert.ToSingle(iTOP_Point.Positions.Y),
                                Z = Convert.ToSingle(iTOP_Point.Positions.Z)
                            },
                            Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                            {
                                X = 10,
                                Y = 10,
                                Z = 10
                            },
                            Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                            {
                                X = 0,
                                Y = 0,
                                Z = 0
                            }
                        };

                        ModelVisual3D dv3D_RouteOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Routes\\Routes.obj");

                        //モデルの名前と番号を文字列に格納(情報化)
                        HTK_3DES.TSRSystem.SetString_MV3D(dv3D_RouteOBJ, "Routes " + iTOP_Point.ID + " " + iTOP_Point.GroupID);

                        HTK_3DES.TransformMV3D.Transform_MV3D(JugemPath_transform_Value, dv3D_RouteOBJ);

                        //AddMDL
                        KMPViewportObject.Routes_List[KMP_Group_ListBox.SelectedIndex].MV3D_List.Add(dv3D_RouteOBJ);

                        render.MainViewPort.Children.Add(dv3D_RouteOBJ);
                        #endregion

                        HTK_3DES.PathTools.ResetRail(render, KMPViewportObject.Routes_List[KMP_Group_ListBox.SelectedIndex], HTK_3DES.PathTools.RailType.Tube);
                        List<Point3D> point3Ds = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.Routes_List[KMP_Group_ListBox.SelectedIndex].MV3D_List);
                        KMPViewportObject.Routes_List[KMP_Group_ListBox.SelectedIndex].TV3D_List = HTK_3DES.PathTools.DrawPath_Tube(render, point3Ds, 10.0, Colors.Blue);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Group : The group may not be selected or it may be empty.");
                    }
                }
                if (KMPSectionComboBox.Text == "Area")
                {
                    KMPPropertyGridSettings.AERA_Section.AERAValue aERAValue = new KMPPropertyGridSettings.AERA_Section.AERAValue
                    {
                        ID = KMP_Path_ListBox.Items.Count,
                        Scales = new KMPPropertyGridSettings.AERA_Section.AERAValue.Scale
                        {
                            X = 1,
                            Y = 1,
                            Z = 1
                        },
                        Rotations = new KMPPropertyGridSettings.AERA_Section.AERAValue.Rotation
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        },
                        Positions = new KMPPropertyGridSettings.AERA_Section.AERAValue.Position
                        {
                            X = (float)Pos.X,
                            Y = (float)Pos.Y,
                            Z = (float)Pos.Z
                        },
                        AreaModeSettings = new KMPPropertyGridSettings.AERA_Section.AERAValue.AreaModeSetting
                        {
                            AreaModeValue = 0
                        },
                        AreaType = 0,
                        AERA_EMACIndex = 0,
                        Priority = 0,
                        AERA_UnkByte1 = 0,
                        AERA_UnkByte2 = 0,
                        RouteID = 0,
                        EnemyID = 0,
                        AERA_UnkByte4 = 0
                    };

                    AERA_Section.AERAValueList.Add(aERAValue);

                    KMP_Path_ListBox.Items.Add(aERAValue);

                    #region Add Model(Area)
                    HTK_3DES.TSRSystem.Transform_Value Area_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                    {
                        Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                        {
                            X = Convert.ToDouble(aERAValue.Positions.X),
                            Y = Convert.ToDouble(aERAValue.Positions.Y),
                            Z = Convert.ToDouble(aERAValue.Positions.Z)
                        },
                        Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                        {
                            X = Convert.ToDouble(aERAValue.Scales.X) * 1000,
                            Y = Convert.ToDouble(aERAValue.Scales.Y) * 1000,
                            Z = Convert.ToDouble(aERAValue.Scales.Z) * 1000
                        },
                        Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                        {
                            X = Convert.ToDouble(aERAValue.Rotations.X),
                            Y = Convert.ToDouble(aERAValue.Rotations.Y),
                            Z = Convert.ToDouble(aERAValue.Rotations.Z)
                        }
                    };

                    ModelVisual3D dv3D_AreaOBJ = null;
                    if (aERAValue.AreaModeSettings.AreaModeValue == 0) dv3D_AreaOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Area\\Area_Box\\Area_Box.obj");

                    //モデルの名前と番号を文字列に格納(情報化)
                    HTK_3DES.TSRSystem.SetString_MV3D(dv3D_AreaOBJ, "Area " + aERAValue.ID + " " + -1);

                    HTK_3DES.TransformMV3D.Transform_MV3D(Area_transform_Value, dv3D_AreaOBJ);

                    //Area_MV3D_List.Add(dv3D_AreaOBJ);
                    KMPViewportObject.Area_MV3DList.Add(dv3D_AreaOBJ);

                    render.MainViewPort.Children.Add(dv3D_AreaOBJ);
                    #endregion
                }
                if (KMPSectionComboBox.Text == "Camera")
                {
                    KMPPropertyGridSettings.EMAC_Section.EMACValue eMACValue = new KMPPropertyGridSettings.EMAC_Section.EMACValue
                    {
                        ID = KMP_Path_ListBox.Items.Count,
                        CameraType = 0,
                        NextCameraIndex = 0,
                        EMAC_UnkBytes1 = 0,
                        EMAC_ITOP_CameraIndex = 0,
                        SpeedSettings = new KMPPropertyGridSettings.EMAC_Section.EMACValue.SpeedSetting
                        {
                            RouteSpeed = 0,
                            FOVSpeed = 0,
                            ViewpointSpeed = 0
                        },
                        EMAC_UnkBytes2 = 0,
                        EMAC_UnkBytes3 = 0,
                        Positions = new KMPPropertyGridSettings.EMAC_Section.EMACValue.Position
                        {
                            X = (float)Pos.X,
                            Y = (float)Pos.Y,
                            Z = (float)Pos.Z
                        },
                        Rotations = new KMPPropertyGridSettings.EMAC_Section.EMACValue.Rotation
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        },
                        FOVAngleSettings = new KMPPropertyGridSettings.EMAC_Section.EMACValue.FOVAngleSetting
                        {
                            FOVAngle_Start = 0,
                            FOVAngle_End = 0
                        },
                        Viewpoint_Destination = new KMPPropertyGridSettings.EMAC_Section.EMACValue.ViewpointDestination
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        },
                        Viewpoint_Start = new KMPPropertyGridSettings.EMAC_Section.EMACValue.ViewpointStart
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        },
                        Camera_Active_Time = 0
                    };

                    EMAC_Section.EMACValueList.Add(eMACValue);

                    KMP_Path_ListBox.Items.Add(eMACValue);

                    #region Add Model(Camera)
                    HTK_3DES.TSRSystem.Transform_Value Camera_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                    {
                        Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                        {
                            X = Convert.ToDouble(eMACValue.Positions.X),
                            Y = Convert.ToDouble(eMACValue.Positions.Y),
                            Z = Convert.ToDouble(eMACValue.Positions.Z)
                        },
                        Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                        {
                            X = 10,
                            Y = 10,
                            Z = 10
                        },
                        Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                        {
                            X = Convert.ToDouble(eMACValue.Rotations.X),
                            Y = Convert.ToDouble(eMACValue.Rotations.Y),
                            Z = Convert.ToDouble(eMACValue.Rotations.Z)
                        }
                    };

                    ModelVisual3D dv3D_CameraOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Camera\\Camera.obj");

                    //モデルの名前と番号を文字列に格納(情報化)
                    HTK_3DES.TSRSystem.SetString_MV3D(dv3D_CameraOBJ, "Camera " + eMACValue.ID + " " + -1);

                    HTK_3DES.TransformMV3D.Transform_MV3D(Camera_transform_Value, dv3D_CameraOBJ);

                    //Camera_MV3D_List.Add(dv3D_CameraOBJ);
                    KMPViewportObject.Camera_MV3DList.Add(dv3D_CameraOBJ);

                    render.MainViewPort.Children.Add(dv3D_CameraOBJ);
                    #endregion
                }
                if (KMPSectionComboBox.Text == "JugemPoint")
                {
                    KMPPropertyGridSettings.TPGJ_Section.TPGJValue tPGJValue = new KMPPropertyGridSettings.TPGJ_Section.TPGJValue
                    {
                        ID = KMP_Path_ListBox.Items.Count,
                        TPGJ_RespawnID = 65535,
                        Positions = new KMPPropertyGridSettings.TPGJ_Section.TPGJValue.Position
                        {
                            X = (float)Pos.X,
                            Y = (float)Pos.Y,
                            Z = (float)Pos.Z
                        },
                        Rotations = new KMPPropertyGridSettings.TPGJ_Section.TPGJValue.Rotation
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        },
                        TPGJ_UnkBytes1 = 0
                    };

                    TPGJ_Section.TPGJValueList.Add(tPGJValue);

                    KMP_Path_ListBox.Items.Add(tPGJValue);

                    #region Add Model(RespawnPoint)
                    HTK_3DES.TSRSystem.Transform_Value RespawnPoint_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                    {
                        Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                        {
                            X = Convert.ToDouble(tPGJValue.Positions.X),
                            Y = Convert.ToDouble(tPGJValue.Positions.Y),
                            Z = Convert.ToDouble(tPGJValue.Positions.Z)
                        },
                        Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                        {
                            X = 10,
                            Y = 10,
                            Z = 10
                        },
                        Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                        {
                            X = Convert.ToDouble(tPGJValue.Rotations.X),
                            Y = Convert.ToDouble(tPGJValue.Rotations.Y),
                            Z = Convert.ToDouble(tPGJValue.Rotations.Z)
                        }
                    };

                    ModelVisual3D dv3D_RespawnPointOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\RespawnPoint\\RespawnPoint.obj");

                    //モデルの名前と番号を文字列に格納(情報化)
                    HTK_3DES.TSRSystem.SetString_MV3D(dv3D_RespawnPointOBJ, "RespawnPoint " + tPGJValue.ID + " " + -1);

                    HTK_3DES.TransformMV3D.Transform_MV3D(RespawnPoint_transform_Value, dv3D_RespawnPointOBJ);

                    //RespawnPoint_MV3D_List.Add(dv3D_RespawnPointOBJ);
                    KMPViewportObject.RespawnPoint_MV3DList.Add(dv3D_RespawnPointOBJ);

                    render.MainViewPort.Children.Add(dv3D_RespawnPointOBJ);
                    #endregion
                }
                if (KMPSectionComboBox.Text == "GlideRoutes")
                {
                    if (KMP_Group_ListBox.SelectedIndex != -1)
                    {
                        KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.TPLGValue tPLGValue = new KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.TPLGValue
                        {
                            GroupID = KMP_Group_ListBox.SelectedIndex,
                            ID = KMP_Path_ListBox.Items.Count,
                            Positions = new KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.TPLGValue.Position
                            {
                                X = (float)Pos.X,
                                Y = (float)Pos.Y,
                                Z = (float)Pos.Z
                            },
                            TPLG_PointScaleValue = 1,
                            TPLG_UnkBytes1 = 0,
                            TPLG_UnkBytes2 = 0
                        };

                        HPLG_TPLG_Section.HPLGValueList[KMP_Group_ListBox.SelectedIndex].TPLGValueList.Add(tPLGValue);

                        KMP_Path_ListBox.Items.Add(tPLGValue);

                        #region Add Model(GlideRoutes)
                        HTK_3DES.TSRSystem.Transform_Value GliderPoint_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                        {
                            Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                            {
                                X = Convert.ToDouble(tPLGValue.Positions.X),
                                Y = Convert.ToDouble(tPLGValue.Positions.Y),
                                Z = Convert.ToDouble(tPLGValue.Positions.Z)
                            },
                            Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                            {
                                X = tPLGValue.TPLG_PointScaleValue * 10,
                                Y = tPLGValue.TPLG_PointScaleValue * 10,
                                Z = tPLGValue.TPLG_PointScaleValue * 10
                            },
                            Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                            {
                                X = 0,
                                Y = 0,
                                Z = 0
                            }
                        };

                        ModelVisual3D dv3D_GliderPathOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\GliderPath\\GliderPath.obj");

                        //モデルの名前と番号を文字列に格納(情報化)
                        HTK_3DES.TSRSystem.SetString_MV3D(dv3D_GliderPathOBJ, "GlideRoutes " + tPLGValue.ID + " " + tPLGValue.GroupID);

                        HTK_3DES.TransformMV3D.Transform_MV3D(GliderPoint_transform_Value, dv3D_GliderPathOBJ);

                        //Add model
                        KMPViewportObject.GlideRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].MV3D_List.Add(dv3D_GliderPathOBJ);

                        render.MainViewPort.Children.Add(dv3D_GliderPathOBJ);
                        #endregion

                        HTK_3DES.PathTools.ResetRail(render, KMPViewportObject.GlideRoute_Rail_List[KMP_Group_ListBox.SelectedIndex], HTK_3DES.PathTools.RailType.Tube);
                        List<Point3D> point3Ds = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.GlideRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].MV3D_List);
                        KMPViewportObject.GlideRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].TV3D_List = HTK_3DES.PathTools.DrawPath_Tube(render, point3Ds, 10.0, Colors.LightSkyBlue);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Group : The group may not be selected or it may be empty.");
                    }
                }
            }
        }

        public void Render_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) == true && Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (HTR != null)
                {
                    string[] MDLStr_GetName = null;
                    if (typeof(ModelVisual3D) == HTR.VisualHit.GetType())
                    {
                        MDLStr_GetName = HTR.VisualHit.GetName().Split(' ');
                    }
                    if (typeof(LinesVisual3D) == HTR.VisualHit.GetType()) return;
                    if (typeof(TubeVisual3D) == HTR.VisualHit.GetType()) return;

                    //string[] MDLStr_GetName = HTR.VisualHit.GetName().Split(' ');

                    #region Get Object info
                    string OBJ_Name = MDLStr_GetName[0];
                    int MDLNum = int.Parse(MDLStr_GetName[1]);
                    int GroupNum = int.Parse(MDLStr_GetName[2]);
                    #endregion

                    if (OBJ_Name == "StartPosition")
                    {
                        //位置を計算
                        Vector3D NewPos = render.Drag(new Vector3D(transform_Value.Translate_Value.X, transform_Value.Translate_Value.Y, transform_Value.Translate_Value.Z), e);

                        #region Moving Axis
                        if (Rad_AxisAll.Checked == true)
                        {
                            //一度Transform_ValueのTranslate_Valueに計算した値を格納
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        if (Rad_AxisX.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisY.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisZ.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        #endregion

                        //Propertyに値を格納する
                        TPTK_Section.TPTKValueList[MDLNum].Position_Value.X = (float)transform_Value.Translate_Value.X;
                        TPTK_Section.TPTKValueList[MDLNum].Position_Value.Y = (float)transform_Value.Translate_Value.Y;
                        TPTK_Section.TPTKValueList[MDLNum].Position_Value.Z = (float)transform_Value.Translate_Value.Z;

                        HTK_3DES.TransformMV3D.Transform_MV3D(transform_Value, FindMV3D, HTK_3DES.TSRSystem.RotationSetting.Angle);

                        //PropertyGridにPropertyを表示させる
                        propertyGrid_KMP_Path.SelectedObject = TPTK_Section.TPTKValueList[MDLNum];
                    }
                    if (OBJ_Name == "EnemyRoute")
                    {
                        Vector3D NewPos = render.Drag(new Vector3D(transform_Value.Translate_Value.X, transform_Value.Translate_Value.Y, transform_Value.Translate_Value.Z), e);

                        #region Moving Axis
                        if (Rad_AxisAll.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        if (Rad_AxisX.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisY.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisZ.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        #endregion

                        //Propertyに値を格納する
                        HPNE_TPNE_Section.HPNEValueList[GroupNum].TPNEValueList[MDLNum].Positions.X = (float)transform_Value.Translate_Value.X;
                        HPNE_TPNE_Section.HPNEValueList[GroupNum].TPNEValueList[MDLNum].Positions.Y = (float)transform_Value.Translate_Value.Y;
                        HPNE_TPNE_Section.HPNEValueList[GroupNum].TPNEValueList[MDLNum].Positions.Z = (float)transform_Value.Translate_Value.Z;

                        HTK_3DES.TransformMV3D.Transform_MV3D(transform_Value, FindMV3D, HTK_3DES.TSRSystem.RotationSetting.Angle);

                        //パスの形を変更
                        HTK_3DES.PathTools.Rail rail = KMPViewportObject.EnemyRoute_Rail_List[GroupNum];
                        Vector3D pos = new Vector3D(transform_Value.Translate_Value.X, transform_Value.Translate_Value.Y, transform_Value.Translate_Value.Z);
                        if (rail.TV3D_List.Count != 0) HTK_3DES.PathTools.MoveRails(MDLNum, pos, rail.TV3D_List);

                        //PropertyGridにPropertyを表示させる
                        propertyGrid_KMP_Path.SelectedObject = HPNE_TPNE_Section.HPNEValueList[GroupNum].TPNEValueList[MDLNum];
                    }
                    if (OBJ_Name == "ItemRoute")
                    {
                        Vector3D NewPos = render.Drag(new Vector3D(transform_Value.Translate_Value.X, transform_Value.Translate_Value.Y, transform_Value.Translate_Value.Z), e);

                        #region Moving Axis
                        if (Rad_AxisAll.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        if (Rad_AxisX.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisY.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisZ.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        #endregion

                        //Propertyに値を格納する
                        HPTI_TPTI_Section.HPTIValueList[GroupNum].TPTIValueList[MDLNum].TPTI_Positions.X = (float)transform_Value.Translate_Value.X;
                        HPTI_TPTI_Section.HPTIValueList[GroupNum].TPTIValueList[MDLNum].TPTI_Positions.Y = (float)transform_Value.Translate_Value.Y;
                        HPTI_TPTI_Section.HPTIValueList[GroupNum].TPTIValueList[MDLNum].TPTI_Positions.Z = (float)transform_Value.Translate_Value.Z;

                        HTK_3DES.TransformMV3D.Transform_MV3D(transform_Value, FindMV3D, HTK_3DES.TSRSystem.RotationSetting.Angle);

                        //パスの形を変更
                        HTK_3DES.PathTools.Rail rail = KMPViewportObject.ItemRoute_Rail_List[GroupNum];
                        Vector3D pos = new Vector3D(transform_Value.Translate_Value.X, transform_Value.Translate_Value.Y, transform_Value.Translate_Value.Z);
                        if (rail.TV3D_List.Count != 0) HTK_3DES.PathTools.MoveRails(MDLNum, pos, rail.TV3D_List);

                        //PropertyGridにPropertyを表示させる
                        propertyGrid_KMP_Path.SelectedObject = HPTI_TPTI_Section.HPTIValueList[GroupNum].TPTIValueList[MDLNum];
                    }
                    if (OBJ_Name == "Checkpoint_Left")
                    {
                        Vector3D NewPos = render.Drag(new Vector3D(transform_Value.Translate_Value.X, Convert.ToDouble(textBox1.Text), transform_Value.Translate_Value.Z), e);

                        #region Moving Axis
                        if (Rad_AxisAll.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        if (Rad_AxisX.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisY.Checked == true)
                        {
                            //Nothing
                        }
                        if (Rad_AxisZ.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        #endregion

                        //Propertyに値を格納する
                        HPKC_TPKC_Section.HPKCValueList[GroupNum].TPKCValueList[MDLNum].Position_2D_Left.X = (float)transform_Value.Translate_Value.X;
                        HPKC_TPKC_Section.HPKCValueList[GroupNum].TPKCValueList[MDLNum].Position_2D_Left.Y = (float)transform_Value.Translate_Value.Z;

                        HTK_3DES.TransformMV3D.Transform_MV3D(transform_Value, FindMV3D, HTK_3DES.TSRSystem.RotationSetting.Angle);

                        //パスの形を変更(機能の追加)
                        HTK_3DES.KMP_3DCheckpointSystem.Checkpoint checkpoint = KMPViewportObject.Checkpoint_Rail[GroupNum];

                        //Green
                        Vector3D pos = new Vector3D(transform_Value.Translate_Value.X, transform_Value.Translate_Value.Y, transform_Value.Translate_Value.Z);
                        if (checkpoint.Checkpoint_Left.LV3D_List.Count != 0) HTK_3DES.PathTools.MoveRails(MDLNum, pos, checkpoint.Checkpoint_Left.LV3D_List);
                        KMPViewportObject.Checkpoint_Rail[GroupNum].Checkpoint_Line[MDLNum].Points[0] = pos.ToPoint3D();

                        //PropertyGridにPropertyを表示させる
                        propertyGrid_KMP_Path.SelectedObject = HPKC_TPKC_Section.HPKCValueList[GroupNum].TPKCValueList[MDLNum];
                    }
                    if (OBJ_Name == "Checkpoint_Right")
                    {
                        Vector3D NewPos = render.Drag(new Vector3D(transform_Value.Translate_Value.X, Convert.ToDouble(textBox1.Text), transform_Value.Translate_Value.Z), e);

                        #region Moving Axis
                        if (Rad_AxisAll.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        if (Rad_AxisX.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisY.Checked == true)
                        {
                            //Nothing
                        }
                        if (Rad_AxisZ.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        #endregion

                        //Propertyに値を格納する
                        HPKC_TPKC_Section.HPKCValueList[GroupNum].TPKCValueList[MDLNum].Position_2D_Right.X = (float)transform_Value.Translate_Value.X;
                        HPKC_TPKC_Section.HPKCValueList[GroupNum].TPKCValueList[MDLNum].Position_2D_Right.Y = (float)transform_Value.Translate_Value.Z;

                        HTK_3DES.TransformMV3D.Transform_MV3D(transform_Value, FindMV3D, HTK_3DES.TSRSystem.RotationSetting.Angle);

                        //パスの形を変更(機能の追加)
                        HTK_3DES.KMP_3DCheckpointSystem.Checkpoint checkpoint = KMPViewportObject.Checkpoint_Rail[GroupNum];

                        //Red
                        Vector3D pos = new Vector3D(transform_Value.Translate_Value.X, transform_Value.Translate_Value.Y, transform_Value.Translate_Value.Z);
                        if (checkpoint.Checkpoint_Right.LV3D_List.Count != 0) HTK_3DES.PathTools.MoveRails(MDLNum, pos, checkpoint.Checkpoint_Right.LV3D_List);
                        KMPViewportObject.Checkpoint_Rail[GroupNum].Checkpoint_Line[MDLNum].Points[1] = pos.ToPoint3D();

                        //PropertyGridにPropertyを表示させる
                        propertyGrid_KMP_Path.SelectedObject = HPKC_TPKC_Section.HPKCValueList[GroupNum].TPKCValueList[MDLNum];
                    }
                    if (OBJ_Name == "OBJ")
                    {
                        //位置を計算
                        Vector3D NewPos = render.Drag(new Vector3D(transform_Value.Translate_Value.X, transform_Value.Translate_Value.Y, transform_Value.Translate_Value.Z), e);

                        #region Moving Axis
                        if (Rad_AxisAll.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        if (Rad_AxisX.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisY.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisZ.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        #endregion

                        //Propertyに値を格納する
                        JBOG_Section.JBOGValueList[MDLNum].Positions.X = (float)transform_Value.Translate_Value.X;
                        JBOG_Section.JBOGValueList[MDLNum].Positions.Y = (float)transform_Value.Translate_Value.Y;
                        JBOG_Section.JBOGValueList[MDLNum].Positions.Z = (float)transform_Value.Translate_Value.Z;

                        HTK_3DES.TransformMV3D.Transform_MV3D(transform_Value, FindMV3D, HTK_3DES.TSRSystem.RotationSetting.Angle);

                        //PropertyGridにPropertyを表示させる
                        propertyGrid_KMP_Path.SelectedObject = JBOG_Section.JBOGValueList[MDLNum];
                    }
                    if (OBJ_Name == "Routes")
                    {
                        //位置を計算
                        Vector3D NewPos = render.Drag(new Vector3D(transform_Value.Translate_Value.X, transform_Value.Translate_Value.Y, transform_Value.Translate_Value.Z), e);

                        #region Moving Axis
                        if (Rad_AxisAll.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        if (Rad_AxisX.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisY.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisZ.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        #endregion

                        //Propertyに値を格納する
                        ITOP_Section.ITOP_RouteList[GroupNum].ITOP_PointList[MDLNum].Positions.X = (float)transform_Value.Translate_Value.X;
                        ITOP_Section.ITOP_RouteList[GroupNum].ITOP_PointList[MDLNum].Positions.Y = (float)transform_Value.Translate_Value.Y;
                        ITOP_Section.ITOP_RouteList[GroupNum].ITOP_PointList[MDLNum].Positions.Z = (float)transform_Value.Translate_Value.Z;

                        HTK_3DES.TransformMV3D.Transform_MV3D(transform_Value, FindMV3D, HTK_3DES.TSRSystem.RotationSetting.Angle);

                        //パスの形を変更
                        HTK_3DES.PathTools.Rail rail = KMPViewportObject.Routes_List[GroupNum];
                        Vector3D pos = new Vector3D(transform_Value.Translate_Value.X, transform_Value.Translate_Value.Y, transform_Value.Translate_Value.Z);
                        if (rail.TV3D_List.Count != 0) HTK_3DES.PathTools.MoveRails(MDLNum, pos, rail.TV3D_List);

                        //PropertyGridにPropertyを表示させる
                        propertyGrid_KMP_Path.SelectedObject = ITOP_Section.ITOP_RouteList[GroupNum].ITOP_PointList[MDLNum];
                    }
                    if (OBJ_Name == "Area")
                    {
                        //位置を計算
                        Vector3D NewPos = render.Drag(new Vector3D(transform_Value.Translate_Value.X, transform_Value.Translate_Value.Y, transform_Value.Translate_Value.Z), e);

                        #region Moving Axis
                        if (Rad_AxisAll.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        if (Rad_AxisX.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisY.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisZ.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        #endregion

                        //Propertyに値を格納する
                        AERA_Section.AERAValueList[MDLNum].Positions.X = (float)transform_Value.Translate_Value.X;
                        AERA_Section.AERAValueList[MDLNum].Positions.Y = (float)transform_Value.Translate_Value.Y;
                        AERA_Section.AERAValueList[MDLNum].Positions.Z = (float)transform_Value.Translate_Value.Z;

                        HTK_3DES.TransformMV3D.Transform_MV3D(transform_Value, FindMV3D, HTK_3DES.TSRSystem.RotationSetting.Angle);

                        //PropertyGridにPropertyを表示させる
                        propertyGrid_KMP_Path.SelectedObject = AERA_Section.AERAValueList[MDLNum];
                    }
                    if (OBJ_Name == "Camera")
                    {
                        //位置を計算
                        Vector3D NewPos = render.Drag(new Vector3D(transform_Value.Translate_Value.X, transform_Value.Translate_Value.Y, transform_Value.Translate_Value.Z), e);

                        #region Moving Axis
                        if (Rad_AxisAll.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        if (Rad_AxisX.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisY.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisZ.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        #endregion

                        //Propertyに値を格納する
                        EMAC_Section.EMACValueList[MDLNum].Positions.X = (float)transform_Value.Translate_Value.X;
                        EMAC_Section.EMACValueList[MDLNum].Positions.Y = (float)transform_Value.Translate_Value.Y;
                        EMAC_Section.EMACValueList[MDLNum].Positions.Z = (float)transform_Value.Translate_Value.Z;

                        HTK_3DES.TransformMV3D.Transform_MV3D(transform_Value, FindMV3D, HTK_3DES.TSRSystem.RotationSetting.Angle);

                        //PropertyGridにPropertyを表示させる
                        propertyGrid_KMP_Path.SelectedObject = EMAC_Section.EMACValueList[MDLNum];
                    }
                    if (OBJ_Name == "RespawnPoint")
                    {
                        //位置を計算
                        Vector3D NewPos = render.Drag(new Vector3D(transform_Value.Translate_Value.X, transform_Value.Translate_Value.Y, transform_Value.Translate_Value.Z), e);

                        #region Moving Axis
                        if (Rad_AxisAll.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        if (Rad_AxisX.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisY.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisZ.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        #endregion

                        //Propertyに値を格納する
                        TPGJ_Section.TPGJValueList[MDLNum].Positions.X = (float)transform_Value.Translate_Value.X;
                        TPGJ_Section.TPGJValueList[MDLNum].Positions.Y = (float)transform_Value.Translate_Value.Y;
                        TPGJ_Section.TPGJValueList[MDLNum].Positions.Z = (float)transform_Value.Translate_Value.Z;

                        HTK_3DES.TransformMV3D.Transform_MV3D(transform_Value, FindMV3D, HTK_3DES.TSRSystem.RotationSetting.Angle);

                        //PropertyGridにPropertyを表示させる
                        propertyGrid_KMP_Path.SelectedObject = TPGJ_Section.TPGJValueList[MDLNum];
                    }
                    if (OBJ_Name == "GlideRoutes")
                    {
                        //位置を計算
                        Vector3D NewPos = render.Drag(new Vector3D(transform_Value.Translate_Value.X, transform_Value.Translate_Value.Y, transform_Value.Translate_Value.Z), e);

                        #region Moving Axis
                        if (Rad_AxisAll.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        if (Rad_AxisX.Checked == true)
                        {
                            transform_Value.Translate_Value.X = NewPos.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisY.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = NewPos.Y;
                            transform_Value.Translate_Value.Z = transform_Value.Translate_Value.Z;
                        }
                        if (Rad_AxisZ.Checked == true)
                        {
                            transform_Value.Translate_Value.X = transform_Value.Translate_Value.X;
                            transform_Value.Translate_Value.Y = transform_Value.Translate_Value.Y;
                            transform_Value.Translate_Value.Z = NewPos.Z;
                        }
                        #endregion

                        //Propertyに値を格納する
                        HPLG_TPLG_Section.HPLGValueList[GroupNum].TPLGValueList[MDLNum].Positions.X = (float)transform_Value.Translate_Value.X;
                        HPLG_TPLG_Section.HPLGValueList[GroupNum].TPLGValueList[MDLNum].Positions.Y = (float)transform_Value.Translate_Value.Y;
                        HPLG_TPLG_Section.HPLGValueList[GroupNum].TPLGValueList[MDLNum].Positions.Z = (float)transform_Value.Translate_Value.Z;

                        HTK_3DES.TransformMV3D.Transform_MV3D(transform_Value, FindMV3D, HTK_3DES.TSRSystem.RotationSetting.Angle);

                        //パスの形を変更
                        HTK_3DES.PathTools.Rail rail = KMPViewportObject.GlideRoute_Rail_List[GroupNum];
                        Vector3D pos = new Vector3D(transform_Value.Translate_Value.X, transform_Value.Translate_Value.Y, transform_Value.Translate_Value.Z);
                        if (rail.TV3D_List.Count != 0) HTK_3DES.PathTools.MoveRails(MDLNum, pos, rail.TV3D_List);

                        //PropertyGridにPropertyを表示させる
                        propertyGrid_KMP_Path.SelectedObject = HPLG_TPLG_Section.HPLGValueList[GroupNum].TPLGValueList[MDLNum];
                    }
                    if (OBJ_Name == "GridLine") return;
                }
            }
        }

        public void render_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //Panel制御
            bool SBool = KMP_Viewport_SplitContainer.Panel2Collapsed;
            if (Keyboard.IsKeyDown(Key.T) == true)
            {
                if (SBool == true)
                {
                    KMP_Viewport_SplitContainer.Panel2Collapsed = false;
                    KMP_Viewport_SplitContainer.Panel2.Show();
                }
                if (SBool == false)
                {
                    KMP_Viewport_SplitContainer.Panel2Collapsed = true;
                    KMP_Viewport_SplitContainer.Panel2.Hide();
                }
            }
        }

        private void readBinaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog Open_KMP = new OpenFileDialog()
            {
                Title = "Open KMP",
                InitialDirectory = @"C:\Users\User\Desktop",
                Filter = "kmp file|*.kmp"
            };

            if (Open_KMP.ShowDialog() != DialogResult.OK) return;

            System.IO.FileStream fs1 = new FileStream(Open_KMP.FileName, FileMode.Open, FileAccess.Read);
            BinaryReader br1 = new BinaryReader(fs1);

            KMPs.KMPFormat KMPFormat = new KMPs.KMPFormat
            {
                DMDCHeader = br1.ReadChars(4),
                FileSize = br1.ReadUInt32(),
                SectionCount = br1.ReadUInt16(),
                DMDCHeaderSize = br1.ReadUInt16(),
                VersionNumber = br1.ReadUInt32(),
                DMDC_SectionOffset = new KMPs.KMPFormat.DMDCSectionOffset
                {
                    TPTK_Offset = br1.ReadUInt32(),
                    TPNE_Offset = br1.ReadUInt32(),
                    HPNE_Offset = br1.ReadUInt32(),
                    TPTI_Offset = br1.ReadUInt32(),
                    HPTI_Offset = br1.ReadUInt32(),
                    TPKC_Offset = br1.ReadUInt32(),
                    HPKC_Offset = br1.ReadUInt32(),
                    JBOG_Offset = br1.ReadUInt32(),
                    ITOP_Offset = br1.ReadUInt32(),
                    AERA_Offset = br1.ReadUInt32(),
                    EMAC_Offset = br1.ReadUInt32(),
                    TPGJ_Offset = br1.ReadUInt32(),
                    TPNC_Offset = br1.ReadUInt32(),
                    TPSM_Offset = br1.ReadUInt32(),
                    IGTS_Offset = br1.ReadUInt32(),
                    SROC_Offset = br1.ReadUInt32(),
                    TPLG_Offset = br1.ReadUInt32(),
                    HPLG_Offset = br1.ReadUInt32()
                },
                KMP_Section = null
            };

            KMPs.KMPFormat.KMPSection KMP_Section = new KMPs.KMPFormat.KMPSection
            {
                TPTK = null,
                TPNE = null,
                HPNE = null,
                TPTI = null,
                HPTI = null,
                TPKC = null,
                HPKC = null,
                JBOG = null,
                ITOP = null,
                AERA = null,
                EMAC = null,
                TPGJ = null,
                TPNC = null,
                TPSM = null,
                IGTS = null,
                SROC = null,
                TPLG = null,
                HPLG = null
            };

            //位置を保存
            long KMPSectionPos = br1.BaseStream.Position;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.TPTK_Offset, SeekOrigin.Current);

            #region TPTK(Kart Point)
            KMPs.KMPFormat.KMPSection.TPTK_Section TPTK = new KMPs.KMPFormat.KMPSection.TPTK_Section
            {
                TPTKHeader = br1.ReadChars(4),
                NumOfEntries = br1.ReadUInt16(),
                AdditionalValue = br1.ReadUInt16(),
                TPTKValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.TPTK_Section.TPTKValue> TPTKValue_List = new List<KMPs.KMPFormat.KMPSection.TPTK_Section.TPTKValue>();

            for (int TPTKCount = 0; TPTKCount < TPTK.NumOfEntries; TPTKCount++)
            {
                KMPs.KMPFormat.KMPSection.TPTK_Section.TPTKValue TPTK_Values = new KMPs.KMPFormat.KMPSection.TPTK_Section.TPTKValue
                {
                    TPTK_Position = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4), br1.ReadBytes(4) }),
                    TPTK_Rotation = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4), br1.ReadBytes(4) }),
                    Player_Index = br1.ReadUInt16(),
                    TPTK_UnkBytes = br1.ReadUInt16()
                };

                TPTKValue_List.Add(TPTK_Values);
            }

            TPTK.TPTKValue_List = TPTKValue_List;
            KMP_Section.TPTK = TPTK;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.TPNE_Offset, SeekOrigin.Current);

            #region TPNE(Enemy Point)
            KMPs.KMPFormat.KMPSection.TPNE_Section TPNE = new KMPs.KMPFormat.KMPSection.TPNE_Section
            {
                TPNEHeader = br1.ReadChars(4),
                NumOfEntries = br1.ReadUInt16(),
                AdditionalValue = br1.ReadUInt16(),
                TPNEValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.TPNE_Section.TPNEValue> TPNEValue_List = new List<KMPs.KMPFormat.KMPSection.TPNE_Section.TPNEValue>();

            for(int TPNECount = 0; TPNECount < TPNE.NumOfEntries; TPNECount++)
            {
                KMPs.KMPFormat.KMPSection.TPNE_Section.TPNEValue TPNE_Values = new KMPs.KMPFormat.KMPSection.TPNE_Section.TPNEValue
                {
                    TPNE_Position = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4), br1.ReadBytes(4) }),
                    Control = br1.ReadSingle(),
                    MushSetting = br1.ReadUInt16(),
                    DriftSetting = br1.ReadByte(),
                    Flags = br1.ReadByte(),
                    PathFindOption = br1.ReadInt16(),
                    MaxSearchYOffset = br1.ReadInt16()
                };

                TPNEValue_List.Add(TPNE_Values);
            }

            TPNE.TPNEValue_List = TPNEValue_List;
            KMP_Section.TPNE = TPNE;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.HPNE_Offset, SeekOrigin.Current);

            #region HPNE(Enemy Path)
            KMPs.KMPFormat.KMPSection.HPNE_Section HPNE = new KMPs.KMPFormat.KMPSection.HPNE_Section
            {
                HPNEHeader = br1.ReadChars(4),
                NumOfEntries = br1.ReadUInt16(),
                AdditionalValue = br1.ReadUInt16(),
                HPNEValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.HPNE_Section.HPNEValue> HPNEValue_List = new List<KMPs.KMPFormat.KMPSection.HPNE_Section.HPNEValue>();

            for(int HPNECount = 0; HPNECount < HPNE.NumOfEntries; HPNECount++)
            {
                KMPs.KMPFormat.KMPSection.HPNE_Section.HPNEValue HPNE_Values = new KMPs.KMPFormat.KMPSection.HPNE_Section.HPNEValue
                {
                    HPNE_StartPoint = br1.ReadUInt16(),
                    HPNE_Length = br1.ReadUInt16(),
                    HPNE_PreviewGroup = new KMPs.KMPFormat.KMPSection.HPNE_Section.HPNEValue.HPNE_PreviewGroups
                    {
                        Prev0 = br1.ReadUInt16(),
                        Prev1 = br1.ReadUInt16(),
                        Prev2 = br1.ReadUInt16(),
                        Prev3 = br1.ReadUInt16(),
                        Prev4 = br1.ReadUInt16(),
                        Prev5 = br1.ReadUInt16(),
                        Prev6 = br1.ReadUInt16(),
                        Prev7 = br1.ReadUInt16(),
                        Prev8 = br1.ReadUInt16(),
                        Prev9 = br1.ReadUInt16(),
                        Prev10 = br1.ReadUInt16(),
                        Prev11 = br1.ReadUInt16(),
                        Prev12 = br1.ReadUInt16(),
                        Prev13 = br1.ReadUInt16(),
                        Prev14 = br1.ReadUInt16(),
                        Prev15 = br1.ReadUInt16()
                    },
                    HPNE_NextGroup = new KMPs.KMPFormat.KMPSection.HPNE_Section.HPNEValue.HPNE_NextGroups
                    {
                        Next0 = br1.ReadUInt16(),
                        Next1 = br1.ReadUInt16(),
                        Next2 = br1.ReadUInt16(),
                        Next3 = br1.ReadUInt16(),
                        Next4 = br1.ReadUInt16(),
                        Next5 = br1.ReadUInt16(),
                        Next6 = br1.ReadUInt16(),
                        Next7 = br1.ReadUInt16(),
                        Next8 = br1.ReadUInt16(),
                        Next9 = br1.ReadUInt16(),
                        Next10 = br1.ReadUInt16(),
                        Next11 = br1.ReadUInt16(),
                        Next12 = br1.ReadUInt16(),
                        Next13 = br1.ReadUInt16(),
                        Next14 = br1.ReadUInt16(),
                        Next15 = br1.ReadUInt16()
                    },
                    HPNE_UnkBytes1 = br1.ReadUInt32()
                };

                HPNEValue_List.Add(HPNE_Values);
            }

            HPNE.HPNEValue_List = HPNEValue_List;
            KMP_Section.HPNE = HPNE;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.TPTI_Offset, SeekOrigin.Current);

            #region TPTI(Item Point)
            KMPs.KMPFormat.KMPSection.TPTI_Section TPTI = new KMPs.KMPFormat.KMPSection.TPTI_Section
            {
                TPTIHeader = br1.ReadChars(4),
                NumOfEntries = br1.ReadUInt16(),
                AdditionalValue = br1.ReadUInt16(),
                TPTIValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.TPTI_Section.TPTIValue> TPTIValue_List = new List<KMPs.KMPFormat.KMPSection.TPTI_Section.TPTIValue>();

            for(int TPTICount = 0; TPTICount < TPTI.NumOfEntries; TPTICount++)
            {
                byte[] BPX = br1.ReadBytes(4);
                byte[] BPY = br1.ReadBytes(4);
                byte[] BPZ = br1.ReadBytes(4);

                KMPs.KMPFormat.KMPSection.TPTI_Section.TPTIValue TPTI_Values = new KMPs.KMPFormat.KMPSection.TPTI_Section.TPTIValue
                {
                    TPTI_Position = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { BPX, BPY, BPZ }),
                    TPTI_PointSize = br1.ReadSingle(),
                    GravityMode = br1.ReadUInt16(),
                    PlayerScanRadius = br1.ReadUInt16()
                };

                TPTIValue_List.Add(TPTI_Values);
            }

            TPTI.TPTIValue_List = TPTIValue_List;
            KMP_Section.TPTI = TPTI;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.HPTI_Offset, SeekOrigin.Current);

            #region HPTI(Item Path)
            KMPs.KMPFormat.KMPSection.HPTI_Section HPTI = new KMPs.KMPFormat.KMPSection.HPTI_Section
            {
                HPTIHeader = br1.ReadChars(4),
                NumOfEntries = br1.ReadUInt16(),
                AdditionalValue = br1.ReadUInt16(),
                HPTIValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.HPTI_Section.HPTIValue> HPTIValue_List = new List<KMPs.KMPFormat.KMPSection.HPTI_Section.HPTIValue>();

            for(int HPTICount = 0; HPTICount < HPTI.NumOfEntries; HPTICount++)
            {
                KMPs.KMPFormat.KMPSection.HPTI_Section.HPTIValue HPTI_Values = new KMPs.KMPFormat.KMPSection.HPTI_Section.HPTIValue
                {
                    HPTI_StartPoint = br1.ReadUInt16(),
                    HPTI_Length = br1.ReadUInt16(),
                    HPTI_PreviewGroup = new KMPs.KMPFormat.KMPSection.HPTI_Section.HPTIValue.HPTI_PreviewGroups
                    {
                        Prev0 = br1.ReadUInt16(),
                        Prev1 = br1.ReadUInt16(),
                        Prev2 = br1.ReadUInt16(),
                        Prev3 = br1.ReadUInt16(),
                        Prev4 = br1.ReadUInt16(),
                        Prev5 = br1.ReadUInt16()
                    },
                    HPTI_NextGroup = new KMPs.KMPFormat.KMPSection.HPTI_Section.HPTIValue.HPTI_NextGroups
                    {
                        Next0 = br1.ReadUInt16(),
                        Next1 = br1.ReadUInt16(),
                        Next2 = br1.ReadUInt16(),
                        Next3 = br1.ReadUInt16(),
                        Next4 = br1.ReadUInt16(),
                        Next5 = br1.ReadUInt16()
                    }
                };

                HPTIValue_List.Add(HPTI_Values);
            }

            HPTI.HPTIValue_List = HPTIValue_List;
            KMP_Section.HPTI = HPTI;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.TPKC_Offset, SeekOrigin.Current);

            #region TPKC(Checkpoint(Point))
            KMPs.KMPFormat.KMPSection.TPKC_Section TPKC = new KMPs.KMPFormat.KMPSection.TPKC_Section
            {
                TPKCHeader = br1.ReadChars(4),
                NumOfEntries = br1.ReadUInt16(),
                AdditionalValue = br1.ReadUInt16(),
                TPKCValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.TPKC_Section.TPKCValue> TPKCValue_List = new List<KMPs.KMPFormat.KMPSection.TPKC_Section.TPKCValue>();

            for(int TPKCCount = 0; TPKCCount < TPKC.NumOfEntries; TPKCCount++)
            {
                KMPs.KMPFormat.KMPSection.TPKC_Section.TPKCValue TPKC_Values = new KMPs.KMPFormat.KMPSection.TPKC_Section.TPKCValue
                {
                    TPKC_2DPosition_Left = KMPs.KMPHelper.Vector3DTo2DConverter.ByteArrayToVector2D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4) }),
                    TPKC_2DPosition_Right = KMPs.KMPHelper.Vector3DTo2DConverter.ByteArrayToVector2D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4) }),
                    TPKC_RespawnID = br1.ReadByte(),
                    TPKC_Checkpoint_Type = br1.ReadByte(),
                    TPKC_PreviousCheckPoint = br1.ReadByte(),
                    TPKC_NextCheckPoint = br1.ReadByte(),
                    TPKC_UnkBytes1 = br1.ReadByte(),
                    TPKC_UnkBytes2 = br1.ReadByte(),
                    TPKC_UnkBytes3 = br1.ReadByte(),
                    TPKC_UnkBytes4 = br1.ReadByte()
                };

                TPKCValue_List.Add(TPKC_Values);
            }

            TPKC.TPKCValue_List = TPKCValue_List;
            KMP_Section.TPKC = TPKC;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.HPKC_Offset, SeekOrigin.Current);

            #region HPKC(Checkpoint(Path))
            KMPs.KMPFormat.KMPSection.HPKC_Section HPKC = new KMPs.KMPFormat.KMPSection.HPKC_Section
            {
                HPKCHeader = br1.ReadChars(4),
                NumOfEntries = br1.ReadUInt16(),
                AdditionalValue = br1.ReadUInt16(),
                HPKCValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.HPKC_Section.HPKCValue> HPKCValue_List = new List<KMPs.KMPFormat.KMPSection.HPKC_Section.HPKCValue>();

            for(int HPKCCount = 0; HPKCCount < HPKC.NumOfEntries; HPKCCount++)
            {
                KMPs.KMPFormat.KMPSection.HPKC_Section.HPKCValue HPKC_Values = new KMPs.KMPFormat.KMPSection.HPKC_Section.HPKCValue
                {
                    HPKC_StartPoint = br1.ReadByte(),
                    HPKC_Length = br1.ReadByte(),
                    HPKC_PreviewGroup = new KMPs.KMPFormat.KMPSection.HPKC_Section.HPKCValue.HPKC_PreviewGroups
                    {
                        Prev0 = br1.ReadByte(),
                        Prev1 = br1.ReadByte(),
                        Prev2 = br1.ReadByte(),
                        Prev3 = br1.ReadByte(),
                        Prev4 = br1.ReadByte(),
                        Prev5 = br1.ReadByte()
                    },
                    HPKC_NextGroup = new KMPs.KMPFormat.KMPSection.HPKC_Section.HPKCValue.HPKC_NextGroups
                    {
                        Next0 = br1.ReadByte(),
                        Next1 = br1.ReadByte(),
                        Next2 = br1.ReadByte(),
                        Next3 = br1.ReadByte(),
                        Next4 = br1.ReadByte(),
                        Next5 = br1.ReadByte()
                    },
                    HPKC_UnkBytes1 = br1.ReadUInt16()
                };

                HPKCValue_List.Add(HPKC_Values);
            }

            HPKC.HPKCValue_List = HPKCValue_List;
            KMP_Section.HPKC = HPKC;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.JBOG_Offset, SeekOrigin.Current);

            #region JBOG(Game Objects)
            KMPs.KMPFormat.KMPSection.JBOG_Section JBOG = new KMPs.KMPFormat.KMPSection.JBOG_Section
            {
                JBOGHeader = br1.ReadChars(4),
                NumOfEntries = br1.ReadUInt16(),
                AdditionalValue = br1.ReadUInt16(),
                JBOGValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.JBOG_Section.JBOGValue> JBOGValue_List = new List<KMPs.KMPFormat.KMPSection.JBOG_Section.JBOGValue>();

            for(int JBOGCount = 0; JBOGCount < JBOG.NumOfEntries; JBOGCount++)
            {
                KMPs.KMPFormat.KMPSection.JBOG_Section.JBOGValue JBOG_Values = new KMPs.KMPFormat.KMPSection.JBOG_Section.JBOGValue
                {
                    ObjectID = br1.ReadBytes(2),
                    JBOG_UnkByte1 = br1.ReadBytes(2),
                    JBOG_Position = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4), br1.ReadBytes(4) }),
                    JBOG_Rotation = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4), br1.ReadBytes(4) }),
                    JBOG_Scale = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4), br1.ReadBytes(4) }),
                    JBOG_ITOP_RouteIDIndex = br1.ReadUInt16(),
                    JOBJ_Specific_Setting = new KMPs.KMPFormat.KMPSection.JBOG_Section.JBOGValue.JBOG_SpecificSetting
                    {
                        Value0 = br1.ReadUInt16(),
                        Value1 = br1.ReadUInt16(),
                        Value2 = br1.ReadUInt16(),
                        Value3 = br1.ReadUInt16(),
                        Value4 = br1.ReadUInt16(),
                        Value5 = br1.ReadUInt16(),
                        Value6 = br1.ReadUInt16(),
                        Value7 = br1.ReadUInt16(),
                    },
                    JBOG_PresenceSetting = br1.ReadUInt16(),
                    JBOG_UnkByte2 = br1.ReadBytes(2),
                    JBOG_UnkByte3 = br1.ReadUInt16()
                };

                JBOGValue_List.Add(JBOG_Values);
            }

            JBOG.JBOGValue_List = JBOGValue_List;
            KMP_Section.JBOG = JBOG;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.ITOP_Offset, SeekOrigin.Current);

            #region ITOP(Routes)
            KMPs.KMPFormat.KMPSection.ITOP_Section ITOP = new KMPs.KMPFormat.KMPSection.ITOP_Section
            {
                ITOPHeader = br1.ReadChars(4),
                ITOP_NumberOfRoute = br1.ReadUInt16(),
                ITOP_NumberOfPoint = br1.ReadUInt16(),
                ITOP_Route_List = null
            };

            List<KMPs.KMPFormat.KMPSection.ITOP_Section.ITOP_Route> ITOP_Route_List = new List<KMPs.KMPFormat.KMPSection.ITOP_Section.ITOP_Route>();

            for(int ITOPRouteCount = 0; ITOPRouteCount < ITOP.ITOP_NumberOfRoute; ITOPRouteCount++)
            {
                KMPs.KMPFormat.KMPSection.ITOP_Section.ITOP_Route ITOP_Routes = new KMPs.KMPFormat.KMPSection.ITOP_Section.ITOP_Route
                {
                    ITOP_Route_NumOfPoint = br1.ReadUInt16(),
                    ITOP_RouteSetting1 = br1.ReadByte(),
                    ITOP_RouteSetting2 = br1.ReadByte(),
                    ITOP_Point_List = null
                };

                List<KMPs.KMPFormat.KMPSection.ITOP_Section.ITOP_Route.ITOP_Point> ITOP_Point_List = new List<KMPs.KMPFormat.KMPSection.ITOP_Section.ITOP_Route.ITOP_Point>();

                for (int ITOP_PointCount = 0; ITOP_PointCount < ITOP_Routes.ITOP_Route_NumOfPoint; ITOP_PointCount++)
                {
                    KMPs.KMPFormat.KMPSection.ITOP_Section.ITOP_Route.ITOP_Point ITOP_Points = new KMPs.KMPFormat.KMPSection.ITOP_Section.ITOP_Route.ITOP_Point
                    {
                        ITOP_Point_Position = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4), br1.ReadBytes(4) }),
                        ITOP_Point_RouteSpeed = br1.ReadUInt16(),
                        ITOP_PointSetting2 = br1.ReadUInt16()
                    };

                    ITOP_Point_List.Add(ITOP_Points);
                }

                ITOP_Routes.ITOP_Point_List = ITOP_Point_List;

                ITOP_Route_List.Add(ITOP_Routes);
            }

            ITOP.ITOP_Route_List = ITOP_Route_List;
            KMP_Section.ITOP = ITOP;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.AERA_Offset, SeekOrigin.Current);

            #region AERA(Area)
            KMPs.KMPFormat.KMPSection.AERA_Section AERA = new KMPs.KMPFormat.KMPSection.AERA_Section
            {
                AERAHeader = br1.ReadChars(4),
                NumOfEntries = br1.ReadUInt16(),
                AdditionalValue = br1.ReadUInt16(),
                AERAValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.AERA_Section.AERAValue> AERAValue_List = new List<KMPs.KMPFormat.KMPSection.AERA_Section.AERAValue>();

            for(int AERACount = 0; AERACount < AERA.NumOfEntries; AERACount++)
            {
                KMPs.KMPFormat.KMPSection.AERA_Section.AERAValue AERA_Values = new KMPs.KMPFormat.KMPSection.AERA_Section.AERAValue
                {
                    AreaMode = br1.ReadByte(),
                    AreaType = br1.ReadByte(),
                    AERA_EMACIndex = br1.ReadByte(),
                    Priority = br1.ReadByte(),
                    AERA_Position = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4), br1.ReadBytes(4) }),
                    AERA_Rotation = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4), br1.ReadBytes(4) }),
                    AERA_Scale = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4), br1.ReadBytes(4) }),
                    AERA_UnkByte1 = br1.ReadUInt16(),
                    AERA_UnkByte2 = br1.ReadUInt16(),
                    RouteID = br1.ReadByte(),
                    EnemyID = br1.ReadByte(),
                    AERA_UnkByte4 = br1.ReadUInt16()
                };

                AERAValue_List.Add(AERA_Values);
            }

            AERA.AERAValue_List = AERAValue_List;
            KMP_Section.AERA = AERA;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.EMAC_Offset, SeekOrigin.Current);

            #region EMAC(Camera)
            KMPs.KMPFormat.KMPSection.EMAC_Section EMAC = new KMPs.KMPFormat.KMPSection.EMAC_Section
            {
                EMACHeader = br1.ReadChars(4),
                NumOfEntries = br1.ReadUInt16(),
                AdditionalValue = br1.ReadUInt16(),
                EMACValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.EMAC_Section.EMACValue> EMACValue_List = new List<KMPs.KMPFormat.KMPSection.EMAC_Section.EMACValue>();

            for(int EMACCount = 0; EMACCount < EMAC.NumOfEntries; EMACCount++)
            {
                KMPs.KMPFormat.KMPSection.EMAC_Section.EMACValue EMAC_Values = new KMPs.KMPFormat.KMPSection.EMAC_Section.EMACValue
                {
                    CameraType = br1.ReadByte(),
                    NextCameraIndex = br1.ReadByte(),
                    EMAC_UnkBytes1 = br1.ReadByte(),
                    EMAC_ITOP_CameraIndex = br1.ReadByte(),
                    RouteSpeed = br1.ReadUInt16(),
                    FOVSpeed = br1.ReadUInt16(),
                    ViewpointSpeed = br1.ReadUInt16(),
                    EMAC_UnkBytes2 = br1.ReadByte(),
                    EMAC_UnkBytes3 = br1.ReadByte(),
                    EMAC_Position = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4) , br1.ReadBytes(4) }),
                    EMAC_Rotation = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4), br1.ReadBytes(4) }),
                    FOVAngle_Start = br1.ReadSingle(),
                    FOVAngle_End = br1.ReadSingle(),
                    Viewpoint_Start = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4), br1.ReadBytes(4) }),
                    Viewpoint_Destination = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4), br1.ReadBytes(4) }),
                    Camera_Active_Time = br1.ReadSingle()
                };

                EMACValue_List.Add(EMAC_Values);
            }

            EMAC.EMACValue_List = EMACValue_List;
            KMP_Section.EMAC = EMAC;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.TPGJ_Offset, SeekOrigin.Current);

            #region TPGJ(Jugem Point)
            KMPs.KMPFormat.KMPSection.TPGJ_Section TPGJ = new KMPs.KMPFormat.KMPSection.TPGJ_Section
            {
                TPGJHeader = br1.ReadChars(4),
                NumOfEntries = br1.ReadUInt16(),
                AdditionalValue = br1.ReadUInt16(),
                TPGJValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.TPGJ_Section.TPGJValue> TPGJValue_List = new List<KMPs.KMPFormat.KMPSection.TPGJ_Section.TPGJValue>();

            for(int TPGJCount = 0; TPGJCount < TPGJ.NumOfEntries; TPGJCount++)
            {
                KMPs.KMPFormat.KMPSection.TPGJ_Section.TPGJValue TPGJ_Values = new KMPs.KMPFormat.KMPSection.TPGJ_Section.TPGJValue
                {
                    TPGJ_Position = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4), br1.ReadBytes(4) }),
                    TPGJ_Rotation = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4), br1.ReadBytes(4) }),
                    TPGJ_RespawnID = br1.ReadUInt16(),
                    TPGJ_UnkBytes1 = br1.ReadUInt16()
                };

                TPGJValue_List.Add(TPGJ_Values);
            }

            TPGJ.TPGJValue_List = TPGJValue_List;
            KMP_Section.TPGJ = TPGJ;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.TPNC_Offset, SeekOrigin.Current);

            #region TPNC(Unused section)
            KMPs.KMPFormat.KMPSection.TPNC_Section TPNC = new KMPs.KMPFormat.KMPSection.TPNC_Section
            {
                TPNCHeader = br1.ReadChars(4),
                NumOfEntries = br1.ReadUInt16(),
            };

            KMP_Section.TPNC = TPNC;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.TPSM_Offset, SeekOrigin.Current);

            #region TPSM(Unused section)
            KMPs.KMPFormat.KMPSection.TPSM_Section TPSM = new KMPs.KMPFormat.KMPSection.TPSM_Section
            {
                TPSMHeader = br1.ReadChars(4),
                NumOfEntries = br1.ReadUInt16(),
            };

            KMP_Section.TPSM = TPSM;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.IGTS_Offset, SeekOrigin.Current);

            #region IGTS(Stage Info)
            KMPs.KMPFormat.KMPSection.IGTS_Section IGTS = new KMPs.KMPFormat.KMPSection.IGTS_Section
            {
                IGTSHeader = br1.ReadChars(4),
                Unknown1 = br1.ReadUInt32(),
                LapCount = br1.ReadByte(),
                PolePosition = br1.ReadByte(),
                Unknown2 = br1.ReadByte(),
                Unknown3 = br1.ReadByte(),
                RGBAColor = new KMPs.KMPFormat.KMPSection.IGTS_Section.RGBA
                {
                    R = br1.ReadByte(),
                    G = br1.ReadByte(),
                    B = br1.ReadByte(),
                    A = br1.ReadByte()
                },
                FlareAlpha = br1.ReadUInt32()
            };

            KMP_Section.IGTS = IGTS;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.SROC_Offset, SeekOrigin.Current);

            #region SROC(Unused section)
            KMPs.KMPFormat.KMPSection.SROC_Section SROC = new KMPs.KMPFormat.KMPSection.SROC_Section
            {
                SROCHeader = br1.ReadChars(4),
                NumOfEntries = br1.ReadUInt16(),
            };

            KMP_Section.SROC = SROC;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.TPLG_Offset, SeekOrigin.Current);

            #region TPLG(Glide Point)
            KMPs.KMPFormat.KMPSection.TPLG_Section TPLG = new KMPs.KMPFormat.KMPSection.TPLG_Section
            {
                TPLGHeader = br1.ReadChars(4),
                NumOfEntries = br1.ReadUInt16(),
                AdditionalValue = br1.ReadUInt16(),
                TPLGValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.TPLG_Section.TPLGValue> TPLGValue_List = new List<KMPs.KMPFormat.KMPSection.TPLG_Section.TPLGValue>();
            
            for(int TPLGCount = 0; TPLGCount < TPLG.NumOfEntries; TPLGCount++)
            {
                KMPs.KMPFormat.KMPSection.TPLG_Section.TPLGValue TPLG_Values = new KMPs.KMPFormat.KMPSection.TPLG_Section.TPLGValue
                {
                    TPLG_Position = KMPs.KMPHelper.ByteArrayToVector3DConverter.ByteArrayToVector3D(new byte[][] { br1.ReadBytes(4), br1.ReadBytes(4), br1.ReadBytes(4) }),
                    TPLG_PointScaleValue = br1.ReadSingle(),
                    TPLG_UnkBytes1 = br1.ReadUInt32(),
                    TPLG_UnkBytes2 = br1.ReadUInt32()
                };

                TPLGValue_List.Add(TPLG_Values);
            }

            TPLG.TPLGValue_List = TPLGValue_List;
            KMP_Section.TPLG = TPLG;
            #endregion

            br1.BaseStream.Position = KMPSectionPos;

            fs1.Seek(KMPFormat.DMDC_SectionOffset.HPLG_Offset, SeekOrigin.Current);

            #region HPLG(Glide Path)
            KMPs.KMPFormat.KMPSection.HPLG_Section HPLG = new KMPs.KMPFormat.KMPSection.HPLG_Section
            {
                HPLGHeader = br1.ReadChars(4),
                NumOfEntries = br1.ReadUInt16(),
                AdditionalValue = br1.ReadUInt16(),
                HPLGValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.HPLG_Section.HPLGValue> HPLGValue_List = new List<KMPs.KMPFormat.KMPSection.HPLG_Section.HPLGValue>();

            for(int HPLGCount = 0; HPLGCount < HPLG.NumOfEntries; HPLGCount++)
            {
                KMPs.KMPFormat.KMPSection.HPLG_Section.HPLGValue HPLG_Values = new KMPs.KMPFormat.KMPSection.HPLG_Section.HPLGValue
                {
                    HPLG_StartPoint = br1.ReadByte(),
                    HPLG_Length = br1.ReadByte(),
                    HPLG_PreviewGroup = new KMPs.KMPFormat.KMPSection.HPLG_Section.HPLGValue.HPLG_PreviewGroups
                    {
                        Prev0 = br1.ReadByte(),
                        Prev1 = br1.ReadByte(),
                        Prev2 = br1.ReadByte(),
                        Prev3 = br1.ReadByte(),
                        Prev4 = br1.ReadByte(),
                        Prev5 = br1.ReadByte()
                    },
                    HPLG_NextGroup = new KMPs.KMPFormat.KMPSection.HPLG_Section.HPLGValue.HPLG_NextGroups
                    {
                        Next0 = br1.ReadByte(),
                        Next1 = br1.ReadByte(),
                        Next2 = br1.ReadByte(),
                        Next3 = br1.ReadByte(),
                        Next4 = br1.ReadByte(),
                        Next5 = br1.ReadByte()
                    },
                    RouteSetting = br1.ReadUInt32(),
                    HPLG_UnkBytes2 = br1.ReadUInt32()
                };

                HPLGValue_List.Add(HPLG_Values);
            }

            HPLG.HPLGValue_List = HPLGValue_List;
            KMP_Section.HPLG = HPLG;
            #endregion

            KMPFormat.KMP_Section = KMP_Section;

            #region Add PropertyGrid

            #region KartPoint
            TPTK_Section = new KMPPropertyGridSettings.TPTK_Section
            {
                TPTKValueList = null
            };

            List<KMPPropertyGridSettings.TPTK_Section.TPTKValue> TPTKValues_List = new List<KMPPropertyGridSettings.TPTK_Section.TPTKValue>(); 

            for(int i = 0; i < TPTK.NumOfEntries; i++)
            {
                KMPPropertyGridSettings.TPTK_Section.TPTKValue tPTKValue = new KMPPropertyGridSettings.TPTK_Section.TPTKValue
                {
                    ID = i,
                    Position_Value = new KMPPropertyGridSettings.TPTK_Section.TPTKValue.Position
                    {
                        X = (float)TPTK.TPTKValue_List[i].TPTK_Position.X,
                        Y = (float)TPTK.TPTKValue_List[i].TPTK_Position.Y,
                        Z = (float)TPTK.TPTKValue_List[i].TPTK_Position.Z
                    },
                    Rotate_Value = new KMPPropertyGridSettings.TPTK_Section.TPTKValue.Rotation
                    {
                        X = HTK_3DES.TSRSystem.RadianToAngle(TPTK.TPTKValue_List[i].TPTK_Rotation.X),
                        Y = HTK_3DES.TSRSystem.RadianToAngle(TPTK.TPTKValue_List[i].TPTK_Rotation.Y),
                        Z = HTK_3DES.TSRSystem.RadianToAngle(TPTK.TPTKValue_List[i].TPTK_Rotation.Z)
                    },
                    Player_Index = TPTK.TPTKValue_List[i].Player_Index,
                    TPTK_UnkBytes = TPTK.TPTKValue_List[i].TPTK_UnkBytes
                };

                TPTKValues_List.Add(tPTKValue);

                #region Add Model(StartPosition)
                HTK_3DES.TSRSystem.Transform_Value StartPosition_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                {
                    Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                    {
                        X = TPTK.TPTKValue_List[i].TPTK_Position.X,
                        Y = TPTK.TPTKValue_List[i].TPTK_Position.Y,
                        Z = TPTK.TPTKValue_List[i].TPTK_Position.Z
                    },
                    Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                    {
                        X = 10,
                        Y = 10,
                        Z = 10
                    },
                    Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                    {
                        X = TPTK.TPTKValue_List[i].TPTK_Rotation.X,
                        Y = TPTK.TPTKValue_List[i].TPTK_Rotation.Y,
                        Z = TPTK.TPTKValue_List[i].TPTK_Rotation.Z
                    }
                };

                ModelVisual3D dv3D_StartPositionOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\StartPosition\\StartPosition.obj");

                //モデルの名前と番号を文字列に格納(情報化)
                HTK_3DES.TSRSystem.SetString_MV3D(dv3D_StartPositionOBJ, "StartPosition " + i + " " + -1);

                HTK_3DES.TransformMV3D.Transform_MV3D(StartPosition_transform_Value, dv3D_StartPositionOBJ);

                KMPViewportObject.StartPosition_MV3DList.Add(dv3D_StartPositionOBJ);

                render.MainViewPort.Children.Add(dv3D_StartPositionOBJ);

                HTK_3DES.TSRSystem.GC_Dispose(dv3D_StartPositionOBJ);
                #endregion
            }

            TPTK_Section.TPTKValueList = TPTKValues_List;
            #endregion

            #region Enemy_Routes
            HPNE_TPNE_Section = new KMPPropertyGridSettings.HPNE_TPNE_Section
            {
                HPNEValueList = null
            };

            List<KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue> HPNEValues_List = new List<KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue>();

            for (int i = 0; i < HPNE.NumOfEntries; i++)
            {
                //Rail
                HTK_3DES.PathTools.Rail KMP_EnemyRoute_Rail = new HTK_3DES.PathTools.Rail
                {
                    TV3D_List = new List<TubeVisual3D>(),
                    MV3D_List = new List<ModelVisual3D>()
                };

                KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue hPNEValue = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue
                {
                    GroupID = i,
                    HPNEPreviewGroups = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.HPNE_PreviewGroups
                    {
                        Prev0 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_PreviewGroup.Prev0,
                        Prev1 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_PreviewGroup.Prev1,
                        Prev2 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_PreviewGroup.Prev2,
                        Prev3 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_PreviewGroup.Prev3,
                        Prev4 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_PreviewGroup.Prev4,
                        Prev5 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_PreviewGroup.Prev5,
                        Prev6 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_PreviewGroup.Prev6,
                        Prev7 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_PreviewGroup.Prev7,
                        Prev8 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_PreviewGroup.Prev8,
                        Prev9 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_PreviewGroup.Prev9,
                        Prev10 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_PreviewGroup.Prev10,
                        Prev11 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_PreviewGroup.Prev11,
                        Prev12 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_PreviewGroup.Prev12,
                        Prev13 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_PreviewGroup.Prev13,
                        Prev14 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_PreviewGroup.Prev14,
                        Prev15 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_PreviewGroup.Prev15,
                    },
                    HPNENextGroups = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.HPNE_NextGroups
                    {
                        Next0 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_NextGroup.Next0,
                        Next1 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_NextGroup.Next1,
                        Next2 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_NextGroup.Next2,
                        Next3 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_NextGroup.Next3,
                        Next4 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_NextGroup.Next4,
                        Next5 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_NextGroup.Next5,
                        Next6 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_NextGroup.Next6,
                        Next7 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_NextGroup.Next7,
                        Next8 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_NextGroup.Next8,
                        Next9 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_NextGroup.Next9,
                        Next10 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_NextGroup.Next10,
                        Next11 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_NextGroup.Next11,
                        Next12 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_NextGroup.Next12,
                        Next13 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_NextGroup.Next13,
                        Next14 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_NextGroup.Next14,
                        Next15 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_NextGroup.Next15,
                    },
                    HPNE_UnkBytes1 = KMPFormat.KMP_Section.HPNE.HPNEValue_List[i].HPNE_UnkBytes1,
                    TPNEValueList = null
                };

                List<KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue> TPNEValues_List = new List<KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue>();

                for (int Count = 0; Count < HPNE.HPNEValue_List[i].HPNE_Length; Count++)
                {
                    KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue tPNEValue = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue
                    {
                        Group_ID = i,
                        ID = Count,
                        Positions = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.Position
                        {
                            X = (float)TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].TPNE_Position.X,
                            Y = (float)TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].TPNE_Position.Y,
                            Z = (float)TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].TPNE_Position.Z
                        },
                        Control = TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].Control,
                        MushSettings = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.MushSetting
                        {
                            MushSettingValue = TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].MushSetting
                        },
                        DriftSettings = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.DriftSetting
                        {
                            DriftSettingValue = TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].DriftSetting
                        },
                        FlagSettings = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.FlagSetting
                        {
                            WideTurn = EnemyRouteFlagConverter.ConvertFlags(TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].Flags, KMPs.KMPHelper.FlagConverter.EnemyRoute.FlagType.WideTurn),
                            NormalTurn = EnemyRouteFlagConverter.ConvertFlags(TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].Flags, KMPs.KMPHelper.FlagConverter.EnemyRoute.FlagType.NormalTurn),
                            SharpTurn = EnemyRouteFlagConverter.ConvertFlags(TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].Flags, KMPs.KMPHelper.FlagConverter.EnemyRoute.FlagType.SharpTurn),
                            TricksForbidden = EnemyRouteFlagConverter.ConvertFlags(TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].Flags, KMPs.KMPHelper.FlagConverter.EnemyRoute.FlagType.TricksForbidden),
                            StickToRoute = EnemyRouteFlagConverter.ConvertFlags(TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].Flags, KMPs.KMPHelper.FlagConverter.EnemyRoute.FlagType.StickToRoute),
                            BouncyMushSection = EnemyRouteFlagConverter.ConvertFlags(TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].Flags, KMPs.KMPHelper.FlagConverter.EnemyRoute.FlagType.BouncyMushSection),
                            ForceDefaultSpeed = EnemyRouteFlagConverter.ConvertFlags(TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].Flags, KMPs.KMPHelper.FlagConverter.EnemyRoute.FlagType.ForceDefaultSpeed),
                            UnknownFlag = EnemyRouteFlagConverter.ConvertFlags(TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].Flags, KMPs.KMPHelper.FlagConverter.EnemyRoute.FlagType.UnknownFlag),
                        },
                        PathFindOptions = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.PathFindOption
                        {
                            PathFindOptionValue = TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].PathFindOption
                        },
                        MaxSearchYOffset = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.MaxSearch_YOffset
                        {
                            MaxSearchYOffsetValue = TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].MaxSearchYOffset
                        }
                    };

                    TPNEValues_List.Add(tPNEValue);

                    #region Add Model(EnemyRoutes)
                    HTK_3DES.TSRSystem.Transform_Value EnemyPoint_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                    {
                        Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                        {
                            X = TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].TPNE_Position.X,
                            Y = TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].TPNE_Position.Y,
                            Z = TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].TPNE_Position.Z
                        },
                        Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                        {
                            X = TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].Control * 100,
                            Y = TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].Control * 100,
                            Z = TPNE.TPNEValue_List[Count + HPNE.HPNEValue_List[i].HPNE_StartPoint].Control * 100
                        },
                        Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        }
                    };

                    ModelVisual3D dv3D_EnemyPathOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\EnemyPath\\EnemyPath.obj");

                    //モデルの名前と番号を文字列に格納(情報化)
                    HTK_3DES.TSRSystem.SetString_MV3D(dv3D_EnemyPathOBJ, "EnemyRoute " + Count + " " + i);

                    HTK_3DES.TransformMV3D.Transform_MV3D(EnemyPoint_transform_Value, dv3D_EnemyPathOBJ);

                    //Add Rail => MV3DList
                    KMP_EnemyRoute_Rail.MV3D_List.Add(dv3D_EnemyPathOBJ);

                    render.MainViewPort.Children.Add(dv3D_EnemyPathOBJ);
                    #endregion
                }

                hPNEValue.TPNEValueList = TPNEValues_List;

                HPNEValues_List.Add(hPNEValue);

                //Add point
                KMPViewportObject.EnemyRoute_Rail_List.Add(KMP_EnemyRoute_Rail);
            }

            HPNE_TPNE_Section.HPNEValueList = HPNEValues_List;

            #region Add Rail
            for (int i = 0; i < KMPViewportObject.EnemyRoute_Rail_List.Count; i++)
            {
                List<Point3D> point3Ds = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.EnemyRoute_Rail_List[i].MV3D_List);
                KMPViewportObject.EnemyRoute_Rail_List[i].TV3D_List = HTK_3DES.PathTools.DrawPath_Tube(render, point3Ds, 10.0, Colors.Orange);
            }
            #endregion

            #endregion

            #region Item Routes
            HPTI_TPTI_Section = new KMPPropertyGridSettings.HPTI_TPTI_Section
            {
                HPTIValueList = null
            };

            List<KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue> HPTIValues_List = new List<KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue>();

            for (int HPTICount = 0; HPTICount < HPTI.NumOfEntries; HPTICount++)
            {
                //Rail
                HTK_3DES.PathTools.Rail KMP_ItemRoute_Rail = new HTK_3DES.PathTools.Rail
                {
                    TV3D_List = new List<TubeVisual3D>(),
                    MV3D_List = new List<ModelVisual3D>()
                };

                KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue hPTIValue = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue
                {
                    GroupID = HPTICount,
                    HPTI_PreviewGroup = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.HPTI_PreviewGroups
                    {
                        Prev0 = KMPFormat.KMP_Section.HPTI.HPTIValue_List[HPTICount].HPTI_PreviewGroup.Prev0,
                        Prev1 = KMPFormat.KMP_Section.HPTI.HPTIValue_List[HPTICount].HPTI_PreviewGroup.Prev1,
                        Prev2 = KMPFormat.KMP_Section.HPTI.HPTIValue_List[HPTICount].HPTI_PreviewGroup.Prev2,
                        Prev3 = KMPFormat.KMP_Section.HPTI.HPTIValue_List[HPTICount].HPTI_PreviewGroup.Prev3,
                        Prev4 = KMPFormat.KMP_Section.HPTI.HPTIValue_List[HPTICount].HPTI_PreviewGroup.Prev4,
                        Prev5 = KMPFormat.KMP_Section.HPTI.HPTIValue_List[HPTICount].HPTI_PreviewGroup.Prev5
                    },
                    HPTI_NextGroup = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.HPTI_NextGroups
                    {
                        Next0 = KMPFormat.KMP_Section.HPTI.HPTIValue_List[HPTICount].HPTI_NextGroup.Next0,
                        Next1 = KMPFormat.KMP_Section.HPTI.HPTIValue_List[HPTICount].HPTI_NextGroup.Next1,
                        Next2 = KMPFormat.KMP_Section.HPTI.HPTIValue_List[HPTICount].HPTI_NextGroup.Next2,
                        Next3 = KMPFormat.KMP_Section.HPTI.HPTIValue_List[HPTICount].HPTI_NextGroup.Next3,
                        Next4 = KMPFormat.KMP_Section.HPTI.HPTIValue_List[HPTICount].HPTI_NextGroup.Next4,
                        Next5 = KMPFormat.KMP_Section.HPTI.HPTIValue_List[HPTICount].HPTI_NextGroup.Next5
                    },
                    TPTIValueList = null
                };

                List<KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue> TPTIVales_List = new List<KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue>();

                for (int Count = 0; Count < HPTI.HPTIValue_List[HPTICount].HPTI_Length; Count++)
                {
                    KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue tPTIValue = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue
                    {
                        Group_ID = HPTICount,
                        ID = Count,
                        TPTI_Positions = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue.TPTI_Position
                        {
                            X = (float)TPTI.TPTIValue_List[Count + HPTI.HPTIValue_List[HPTICount].HPTI_StartPoint].TPTI_Position.X,
                            Y = (float)TPTI.TPTIValue_List[Count + HPTI.HPTIValue_List[HPTICount].HPTI_StartPoint].TPTI_Position.Y,
                            Z = (float)TPTI.TPTIValue_List[Count + HPTI.HPTIValue_List[HPTICount].HPTI_StartPoint].TPTI_Position.Z
                        },
                        TPTI_PointSize = TPTI.TPTIValue_List[Count + HPTI.HPTIValue_List[HPTICount].HPTI_StartPoint].TPTI_PointSize,
                        GravityModeSettings = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue.GravityModeSetting
                        {
                            GravityModeValue = TPTI.TPTIValue_List[Count + HPTI.HPTIValue_List[HPTICount].HPTI_StartPoint].GravityMode
                        },
                        PlayerScanRadiusSettings = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue.PlayerScanRadiusSetting
                        {
                            PlayerScanRadiusValue = TPTI.TPTIValue_List[Count + HPTI.HPTIValue_List[HPTICount].HPTI_StartPoint].PlayerScanRadius
                        }
                    };

                    TPTIVales_List.Add(tPTIValue);

                    #region Add Model(ItemRoutes)
                    HTK_3DES.TSRSystem.Transform_Value ItemPoint_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                    {
                        Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                        {
                            X = TPTI.TPTIValue_List[Count + HPTI.HPTIValue_List[HPTICount].HPTI_StartPoint].TPTI_Position.X,
                            Y = TPTI.TPTIValue_List[Count + HPTI.HPTIValue_List[HPTICount].HPTI_StartPoint].TPTI_Position.Y,
                            Z = TPTI.TPTIValue_List[Count + HPTI.HPTIValue_List[HPTICount].HPTI_StartPoint].TPTI_Position.Z
                        },
                        Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                        {
                            X = TPTI.TPTIValue_List[Count + HPTI.HPTIValue_List[HPTICount].HPTI_StartPoint].TPTI_PointSize * 100,
                            Y = TPTI.TPTIValue_List[Count + HPTI.HPTIValue_List[HPTICount].HPTI_StartPoint].TPTI_PointSize * 100,
                            Z = TPTI.TPTIValue_List[Count + HPTI.HPTIValue_List[HPTICount].HPTI_StartPoint].TPTI_PointSize * 100
                        },
                        Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        }
                    };

                    ModelVisual3D dv3D_ItemPathOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\ItemPath\\ItemPath.obj");

                    //モデルの名前と番号を文字列に格納(情報化)
                    HTK_3DES.TSRSystem.SetString_MV3D(dv3D_ItemPathOBJ, "ItemRoute " + Count + " " + HPTICount);

                    HTK_3DES.TransformMV3D.Transform_MV3D(ItemPoint_transform_Value, dv3D_ItemPathOBJ);

                    //Add Rail => MV3DList
                    KMP_ItemRoute_Rail.MV3D_List.Add(dv3D_ItemPathOBJ);

                    render.MainViewPort.Children.Add(dv3D_ItemPathOBJ);
                    #endregion
                }

                hPTIValue.TPTIValueList = TPTIVales_List;

                HPTIValues_List.Add(hPTIValue);

                //Add point
                KMPViewportObject.ItemRoute_Rail_List.Add(KMP_ItemRoute_Rail);
            }

            HPTI_TPTI_Section.HPTIValueList = HPTIValues_List;

            #region Add Rail
            for (int i = 0; i < KMPViewportObject.ItemRoute_Rail_List.Count; i++)
            {
                List<Point3D> point3Ds = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.ItemRoute_Rail_List[i].MV3D_List);
                KMPViewportObject.ItemRoute_Rail_List[i].TV3D_List = HTK_3DES.PathTools.DrawPath_Tube(render, point3Ds, 10.0, Colors.Green);
            }
            #endregion

            #endregion

            #region CheckPoint
            //Checkpoint_List
            List<HTK_3DES.KMP_3DCheckpointSystem.Checkpoint> Checkpoints_List = new List<HTK_3DES.KMP_3DCheckpointSystem.Checkpoint>();

            HPKC_TPKC_Section = new KMPPropertyGridSettings.HPKC_TPKC_Section
            {
                HPKCValueList = null
            };

            List<KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue> HPKCValues_List = new List<KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue>();

            for (int HPKCCount = 0; HPKCCount < HPKC.NumOfEntries; HPKCCount++)
            {
                //Checkpoint_Rails
                HTK_3DES.KMP_3DCheckpointSystem.Checkpoint checkpoint = new HTK_3DES.KMP_3DCheckpointSystem.Checkpoint
                {
                    Checkpoint_Left = new HTK_3DES.PathTools.Rail
                    {
                        LV3D_List = new List<LinesVisual3D>(),
                        TV3D_List = new List<TubeVisual3D>(),
                        MV3D_List = new List<ModelVisual3D>()
                    },
                    Checkpoint_Right = new HTK_3DES.PathTools.Rail
                    {
                        LV3D_List = new List<LinesVisual3D>(),
                        TV3D_List = new List<TubeVisual3D>(),
                        MV3D_List = new List<ModelVisual3D>()
                    },
                    Checkpoint_Line = new List<LinesVisual3D>(),
                    Checkpoint_Tube = new List<TubeVisual3D>()
                };

                KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue hPKCValue = new KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue
                {
                    GroupID = HPKCCount,
                    HPKC_PreviewGroup = new KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.HPKC_PreviewGroups
                    {
                        Prev0 = KMPFormat.KMP_Section.HPKC.HPKCValue_List[HPKCCount].HPKC_PreviewGroup.Prev0,
                        Prev1 = KMPFormat.KMP_Section.HPKC.HPKCValue_List[HPKCCount].HPKC_PreviewGroup.Prev1,
                        Prev2 = KMPFormat.KMP_Section.HPKC.HPKCValue_List[HPKCCount].HPKC_PreviewGroup.Prev2,
                        Prev3 = KMPFormat.KMP_Section.HPKC.HPKCValue_List[HPKCCount].HPKC_PreviewGroup.Prev3,
                        Prev4 = KMPFormat.KMP_Section.HPKC.HPKCValue_List[HPKCCount].HPKC_PreviewGroup.Prev4,
                        Prev5 = KMPFormat.KMP_Section.HPKC.HPKCValue_List[HPKCCount].HPKC_PreviewGroup.Prev5
                    },
                    HPKC_NextGroup = new KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.HPKC_NextGroups
                    {
                        Next0 = KMPFormat.KMP_Section.HPKC.HPKCValue_List[HPKCCount].HPKC_NextGroup.Next0,
                        Next1 = KMPFormat.KMP_Section.HPKC.HPKCValue_List[HPKCCount].HPKC_NextGroup.Next1,
                        Next2 = KMPFormat.KMP_Section.HPKC.HPKCValue_List[HPKCCount].HPKC_NextGroup.Next2,
                        Next3 = KMPFormat.KMP_Section.HPKC.HPKCValue_List[HPKCCount].HPKC_NextGroup.Next3,
                        Next4 = KMPFormat.KMP_Section.HPKC.HPKCValue_List[HPKCCount].HPKC_NextGroup.Next4,
                        Next5 = KMPFormat.KMP_Section.HPKC.HPKCValue_List[HPKCCount].HPKC_NextGroup.Next5
                    },
                    HPKC_UnkBytes1 = KMPFormat.KMP_Section.HPKC.HPKCValue_List[HPKCCount].HPKC_UnkBytes1,
                    TPKCValueList = null
                };

                List<KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue> TPKCValues_List = new List<KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue>();

                for (int Count = 0; Count < HPKC.HPKCValue_List[HPKCCount].HPKC_Length; Count++)
                {
                    KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue tPKCValue = new KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue
                    {
                        Group_ID = HPKCCount,
                        ID = Count,
                        Position_2D_Left = new KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue.Position2D_Left
                        {
                            X = (float)TPKC.TPKCValue_List[Count + HPKC.HPKCValue_List[HPKCCount].HPKC_StartPoint].TPKC_2DPosition_Left.X,
                            Y = (float)TPKC.TPKCValue_List[Count + HPKC.HPKCValue_List[HPKCCount].HPKC_StartPoint].TPKC_2DPosition_Left.Y
                        },
                        Position_2D_Right = new KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue.Position2D_Right
                        {
                            X = (float)TPKC.TPKCValue_List[Count + HPKC.HPKCValue_List[HPKCCount].HPKC_StartPoint].TPKC_2DPosition_Right.X,
                            Y = (float)TPKC.TPKCValue_List[Count + HPKC.HPKCValue_List[HPKCCount].HPKC_StartPoint].TPKC_2DPosition_Right.Y
                        },
                        TPKC_RespawnID = TPKC.TPKCValue_List[Count + HPKC.HPKCValue_List[HPKCCount].HPKC_StartPoint].TPKC_RespawnID,
                        TPKC_Checkpoint_Type = TPKC.TPKCValue_List[Count + HPKC.HPKCValue_List[HPKCCount].HPKC_StartPoint].TPKC_Checkpoint_Type,
                        TPKC_PreviousCheckPoint = TPKC.TPKCValue_List[Count + HPKC.HPKCValue_List[HPKCCount].HPKC_StartPoint].TPKC_PreviousCheckPoint,
                        TPKC_NextCheckPoint = TPKC.TPKCValue_List[Count + HPKC.HPKCValue_List[HPKCCount].HPKC_StartPoint].TPKC_NextCheckPoint,
                        TPKC_UnkBytes1 = TPKC.TPKCValue_List[Count + HPKC.HPKCValue_List[HPKCCount].HPKC_StartPoint].TPKC_UnkBytes1,
                        TPKC_UnkBytes2 = TPKC.TPKCValue_List[Count + HPKC.HPKCValue_List[HPKCCount].HPKC_StartPoint].TPKC_UnkBytes2,
                        TPKC_UnkBytes3 = TPKC.TPKCValue_List[Count + HPKC.HPKCValue_List[HPKCCount].HPKC_StartPoint].TPKC_UnkBytes3,
                        TPKC_UnkBytes4 = TPKC.TPKCValue_List[Count + HPKC.HPKCValue_List[HPKCCount].HPKC_StartPoint].TPKC_UnkBytes4
                    };

                    TPKCValues_List.Add(tPKCValue);

                    #region Create
                    var P2D_Left = tPKCValue.Position_2D_Left;
                    Vector2 P2DLeftToVector2 = new Vector2(Convert.ToSingle(P2D_Left.X), Convert.ToSingle(P2D_Left.Y));
                    Point3D P3DLeft = KMPs.KMPHelper.Vector3DTo2DConverter.Vector2DTo3D(P2DLeftToVector2, KMPs.KMPHelper.Vector3DTo2DConverter.Axis_Up.Y).ToPoint3D();
                    P3DLeft.Y = Convert.ToDouble(textBox1.Text);

                    #region Transform(Left)
                    HTK_3DES.TSRSystem.Transform_Value P2DLeft_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                    {
                        Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                        {
                            X = P3DLeft.X,
                            Y = P3DLeft.Y,
                            Z = P3DLeft.Z
                        },
                        Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                        {
                            X = 50,
                            Y = 50,
                            Z = 50
                        },
                        Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        }
                    };

                    ModelVisual3D dv3D_CheckpointLeftOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Checkpoint\\LeftPoint\\Checkpoint_Left.obj");

                    //モデルの名前と番号を文字列に格納(情報化)
                    HTK_3DES.TSRSystem.SetString_MV3D(dv3D_CheckpointLeftOBJ, "Checkpoint_Left " + Count + " " + HPKCCount);

                    HTK_3DES.TransformMV3D.Transform_MV3D(P2DLeft_transform_Value, dv3D_CheckpointLeftOBJ);

                    checkpoint.Checkpoint_Left.MV3D_List.Add(dv3D_CheckpointLeftOBJ);

                    render.MainViewPort.Children.Add(dv3D_CheckpointLeftOBJ);

                    HTK_3DES.TSRSystem.GC_Dispose(dv3D_CheckpointLeftOBJ);
                    #endregion

                    var P2D_Right = tPKCValue.Position_2D_Right;
                    Vector2 P2DRightToVector2 = new Vector2(Convert.ToSingle(P2D_Right.X), Convert.ToSingle(P2D_Right.Y));
                    Point3D P3DRight = KMPs.KMPHelper.Vector3DTo2DConverter.Vector2DTo3D(P2DRightToVector2, KMPs.KMPHelper.Vector3DTo2DConverter.Axis_Up.Y).ToPoint3D();
                    P3DRight.Y = Convert.ToDouble(textBox1.Text);

                    #region Transform(Right)
                    HTK_3DES.TSRSystem.Transform_Value P2DRight_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                    {
                        Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                        {
                            X = P3DRight.X,
                            Y = P3DRight.Y,
                            Z = P3DRight.Z
                        },
                        Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                        {
                            X = 50,
                            Y = 50,
                            Z = 50
                        },
                        Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        }
                    };

                    ModelVisual3D dv3D_CheckpointRightOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Checkpoint\\RightPoint\\Checkpoint_Right.obj");

                    //モデルの名前と番号を文字列に格納(情報化)
                    HTK_3DES.TSRSystem.SetString_MV3D(dv3D_CheckpointRightOBJ, "Checkpoint_Right " + Count + " " + HPKCCount);

                    HTK_3DES.TransformMV3D.Transform_MV3D(P2DRight_transform_Value, dv3D_CheckpointRightOBJ);

                    checkpoint.Checkpoint_Right.MV3D_List.Add(dv3D_CheckpointRightOBJ);

                    render.MainViewPort.Children.Add(dv3D_CheckpointRightOBJ);

                    HTK_3DES.TSRSystem.GC_Dispose(dv3D_CheckpointRightOBJ);
                    #endregion

                    List<Point3D> point3Ds = new List<Point3D>();
                    point3Ds.Add(P3DLeft);
                    point3Ds.Add(P3DRight);

                    LinesVisual3D linesVisual3D = new LinesVisual3D
                    {
                        Points = new Point3DCollection(point3Ds),
                        Thickness = 1,
                        Color = Colors.Black
                    };

                    checkpoint.Checkpoint_Line.Add(linesVisual3D);
                    render.MainViewPort.Children.Add(linesVisual3D);
                    #endregion
                }

                hPKCValue.TPKCValueList = TPKCValues_List;

                HPKCValues_List.Add(hPKCValue);

                //Add Checkpoint
                Checkpoints_List.Add(checkpoint);
            }

            HPKC_TPKC_Section.HPKCValueList = HPKCValues_List;

            KMPViewportObject.Checkpoint_Rail = Checkpoints_List;

            #region Add Rail
            for (int i = 0; i < KMPViewportObject.Checkpoint_Rail.Count; i++)
            {
                List<Point3D> point3Ds_Left = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.Checkpoint_Rail[i].Checkpoint_Left.MV3D_List);
                KMPViewportObject.Checkpoint_Rail[i].Checkpoint_Left.LV3D_List = HTK_3DES.PathTools.DrawPath_Line(render, point3Ds_Left, 5, Colors.Green);

                List<Point3D> point3Ds_Right = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.Checkpoint_Rail[i].Checkpoint_Right.MV3D_List);
                KMPViewportObject.Checkpoint_Rail[i].Checkpoint_Right.LV3D_List = HTK_3DES.PathTools.DrawPath_Line(render, point3Ds_Right, 5, Colors.Red);
            }
            #endregion

            #endregion

            #region OBJ
            JBOG_Section = new KMPPropertyGridSettings.JBOG_section
            {
                JBOGValueList = null
            };

            List<KMPPropertyGridSettings.JBOG_section.JBOGValue> JBOGValues_List = new List<KMPPropertyGridSettings.JBOG_section.JBOGValue>();

            for (int Count = 0; Count < JBOG.NumOfEntries; Count++)
            {
                KMPs.KMPHelper.ObjFlowReader.ObjFlowXmlToObject objFlowXmlToObject_FindName = KMPs.KMPHelper.ObjFlowReader.ReadObjFlowXml("ObjFlowData.xml");
                string Name = objFlowXmlToObject_FindName.ObjFlows.Find(x => x.ObjectID == BitConverter.ToString(KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].ObjectID.Reverse().ToArray()).Replace("-", string.Empty)).ObjectName;

                KMPPropertyGridSettings.JBOG_section.JBOGValue jBOGValue = new KMPPropertyGridSettings.JBOG_section.JBOGValue
                {
                    ID = Count,
                    ObjectName = Name,
                    ObjectID = BitConverter.ToString(KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].ObjectID.Reverse().ToArray()).Replace("-", string.Empty),
                    JBOG_UnkByte1 = BitConverter.ToString(KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_UnkByte1.Reverse().ToArray()).Replace("-", string.Empty),
                    Positions = new KMPPropertyGridSettings.JBOG_section.JBOGValue.Position
                    {
                        X = (float)KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Position.X,
                        Y = (float)KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Position.Y,
                        Z = (float)KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Position.Z
                    },
                    Rotations = new KMPPropertyGridSettings.JBOG_section.JBOGValue.Rotation
                    {
                        X = HTK_3DES.TSRSystem.RadianToAngle(KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Rotation.X),
                        Y = HTK_3DES.TSRSystem.RadianToAngle(KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Rotation.Y),
                        Z = HTK_3DES.TSRSystem.RadianToAngle(KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Rotation.Z)
                    },
                    Scales = new KMPPropertyGridSettings.JBOG_section.JBOGValue.Scale
                    {
                        X = (float)KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Scale.X,
                        Y = (float)KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Scale.Y,
                        Z = (float)KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Scale.Z
                    },
                    JBOG_ITOP_RouteIDIndex = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_ITOP_RouteIDIndex,
                    JOBJ_Specific_Setting = new KMPPropertyGridSettings.JBOG_section.JBOGValue.JBOG_SpecificSetting
                    {
                        Value0 = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JOBJ_Specific_Setting.Value0,
                        Value1 = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JOBJ_Specific_Setting.Value1,
                        Value2 = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JOBJ_Specific_Setting.Value2,
                        Value3 = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JOBJ_Specific_Setting.Value3,
                        Value4 = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JOBJ_Specific_Setting.Value4,
                        Value5 = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JOBJ_Specific_Setting.Value5,
                        Value6 = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JOBJ_Specific_Setting.Value6,
                        Value7 = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JOBJ_Specific_Setting.Value7
                    },
                    JBOG_PresenceSetting = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_PresenceSetting,
                    JBOG_UnkByte2 = BitConverter.ToString(KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_UnkByte2.Reverse().ToArray()).Replace("-", string.Empty),
                    JBOG_UnkByte3 = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_UnkByte3
                };

                JBOGValues_List.Add(jBOGValue);

                #region Add Model(OBJ)
                HTK_3DES.TSRSystem.Transform_Value OBJ_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                {
                    Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                    {
                        X = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Position.X,
                        Y = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Position.Y,
                        Z = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Position.Z
                    },
                    Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                    {
                        X = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Scale.X * 2,
                        Y = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Scale.Y * 2,
                        Z = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Scale.Z * 2
                    },
                    Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                    {
                        X = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Rotation.X,
                        Y = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Rotation.Y,
                        Z = KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].JBOG_Rotation.Z
                    }
                };

                KMPs.KMPHelper.ObjFlowReader.ObjFlowXmlToObject objFlowXmlToObject = KMPs.KMPHelper.ObjFlowReader.ReadObjFlowXml("ObjFlowData.xml");
                string Path = objFlowXmlToObject.ObjFlows.Find(x => x.ObjectID == BitConverter.ToString(KMPFormat.KMP_Section.JBOG.JBOGValue_List[Count].ObjectID.Reverse().ToArray()).Replace("-", string.Empty)).Path;
                ModelVisual3D dv3D_OBJ = HTK_3DES.TSRSystem.OBJReader(Path);

                //モデルの名前と番号を文字列に格納(情報化)
                HTK_3DES.TSRSystem.SetString_MV3D(dv3D_OBJ, "OBJ " + Count + " " + -1);

                HTK_3DES.TransformMV3D.Transform_MV3D(OBJ_transform_Value, dv3D_OBJ);

                KMPViewportObject.OBJ_MV3DList.Add(dv3D_OBJ);

                render.MainViewPort.Children.Add(dv3D_OBJ);
                #endregion
            }

            JBOG_Section.JBOGValueList = JBOGValues_List;
            #endregion

            #region Route
            ITOP_Section = new KMPPropertyGridSettings.ITOP_Section
            {
                ITOP_RouteList = null
            };

            List<KMPPropertyGridSettings.ITOP_Section.ITOP_Route> ITOPRoutes_List = new List<KMPPropertyGridSettings.ITOP_Section.ITOP_Route>();

            for (int ITOP_RoutesCount = 0; ITOP_RoutesCount < ITOP.ITOP_NumberOfRoute; ITOP_RoutesCount++)
            {
                //Rail
                HTK_3DES.PathTools.Rail Route_Rail = new HTK_3DES.PathTools.Rail
                {
                    TV3D_List = new List<TubeVisual3D>(),
                    MV3D_List = new List<ModelVisual3D>()
                };

                KMPPropertyGridSettings.ITOP_Section.ITOP_Route ITOPRoute = new KMPPropertyGridSettings.ITOP_Section.ITOP_Route
                {
                    GroupID = ITOP_RoutesCount,
                    ITOP_RouteSetting1 = ITOP.ITOP_Route_List[ITOP_RoutesCount].ITOP_RouteSetting1,
                    ITOP_RouteSetting2 = ITOP.ITOP_Route_List[ITOP_RoutesCount].ITOP_RouteSetting2,
                    ITOP_PointList = null
                };

                List<KMPPropertyGridSettings.ITOP_Section.ITOP_Route.ITOP_Point> ITOPPoints_List = new List<KMPPropertyGridSettings.ITOP_Section.ITOP_Route.ITOP_Point>();

                for (int ITOP_PointsCount = 0; ITOP_PointsCount < ITOP.ITOP_Route_List[ITOP_RoutesCount].ITOP_Route_NumOfPoint; ITOP_PointsCount++)
                {
                    KMPPropertyGridSettings.ITOP_Section.ITOP_Route.ITOP_Point ITOPPoint = new KMPPropertyGridSettings.ITOP_Section.ITOP_Route.ITOP_Point
                    {
                        GroupID = ITOP_RoutesCount,
                        ID = ITOP_PointsCount,
                        Positions = new KMPPropertyGridSettings.ITOP_Section.ITOP_Route.ITOP_Point.Position
                        {
                            X = (float)ITOP.ITOP_Route_List[ITOP_RoutesCount].ITOP_Point_List[ITOP_PointsCount].ITOP_Point_Position.X,
                            Y = (float)ITOP.ITOP_Route_List[ITOP_RoutesCount].ITOP_Point_List[ITOP_PointsCount].ITOP_Point_Position.Y,
                            Z = (float)ITOP.ITOP_Route_List[ITOP_RoutesCount].ITOP_Point_List[ITOP_PointsCount].ITOP_Point_Position.Z
                        },
                        ITOP_Point_RouteSpeed = ITOP.ITOP_Route_List[ITOP_RoutesCount].ITOP_Point_List[ITOP_PointsCount].ITOP_Point_RouteSpeed,
                        ITOP_PointSetting2 = ITOP.ITOP_Route_List[ITOP_RoutesCount].ITOP_Point_List[ITOP_PointsCount].ITOP_PointSetting2
                    };

                    ITOPPoints_List.Add(ITOPPoint);

                    #region Add Model(Routes)
                    HTK_3DES.TSRSystem.Transform_Value JugemPath_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                    {
                        Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                        {
                            X = Convert.ToSingle(ITOP.ITOP_Route_List[ITOP_RoutesCount].ITOP_Point_List[ITOP_PointsCount].ITOP_Point_Position.X),
                            Y = Convert.ToSingle(ITOP.ITOP_Route_List[ITOP_RoutesCount].ITOP_Point_List[ITOP_PointsCount].ITOP_Point_Position.Y),
                            Z = Convert.ToSingle(ITOP.ITOP_Route_List[ITOP_RoutesCount].ITOP_Point_List[ITOP_PointsCount].ITOP_Point_Position.Z)
                        },
                        Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                        {
                            X = 10,
                            Y = 10,
                            Z = 10
                        },
                        Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        }
                    };

                    ModelVisual3D dv3D_RouteOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Routes\\Routes.obj");

                    //モデルの名前と番号を文字列に格納(情報化)
                    HTK_3DES.TSRSystem.SetString_MV3D(dv3D_RouteOBJ, "Routes " + ITOP_PointsCount + " " + ITOP_RoutesCount);

                    HTK_3DES.TransformMV3D.Transform_MV3D(JugemPath_transform_Value, dv3D_RouteOBJ);

                    //AddMDL
                    Route_Rail.MV3D_List.Add(dv3D_RouteOBJ);

                    render.MainViewPort.Children.Add(dv3D_RouteOBJ);
                    #endregion
                }

                ITOPRoute.ITOP_PointList = ITOPPoints_List;
                ITOPRoutes_List.Add(ITOPRoute);

                KMPViewportObject.Routes_List.Add(Route_Rail);
            }

            ITOP_Section.ITOP_RouteList = ITOPRoutes_List;

            #region Add Rail
            for (int i = 0; i < KMPViewportObject.Routes_List.Count; i++)
            {
                List<Point3D> point3Ds = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.Routes_List[i].MV3D_List);
                KMPViewportObject.Routes_List[i].TV3D_List = HTK_3DES.PathTools.DrawPath_Tube(render, point3Ds, 10.0, Colors.Blue);
            }
            #endregion

            #endregion

            #region Area
            AERA_Section = new KMPPropertyGridSettings.AERA_Section
            {
                AERAValueList = null
            };

            List<KMPPropertyGridSettings.AERA_Section.AERAValue> AERAValues_List = new List<KMPPropertyGridSettings.AERA_Section.AERAValue>();

            for(int AERACount = 0; AERACount < AERA.NumOfEntries; AERACount++)
            {
                KMPPropertyGridSettings.AERA_Section.AERAValue AERAValue = new KMPPropertyGridSettings.AERA_Section.AERAValue
                {
                    ID = AERACount,
                    AreaModeSettings = new KMPPropertyGridSettings.AERA_Section.AERAValue.AreaModeSetting
                    {
                        AreaModeValue = AERA.AERAValue_List[AERACount].AreaMode
                    },
                    AreaType = AERA.AERAValue_List[AERACount].AreaType,
                    AERA_EMACIndex = AERA.AERAValue_List[AERACount].AERA_EMACIndex,
                    Priority = AERA.AERAValue_List[AERACount].Priority,
                    Positions = new KMPPropertyGridSettings.AERA_Section.AERAValue.Position
                    {
                        X = (float)AERA.AERAValue_List[AERACount].AERA_Position.X,
                        Y = (float)AERA.AERAValue_List[AERACount].AERA_Position.Y,
                        Z = (float)AERA.AERAValue_List[AERACount].AERA_Position.Z
                    },
                    Rotations = new KMPPropertyGridSettings.AERA_Section.AERAValue.Rotation
                    {
                        X = HTK_3DES.TSRSystem.RadianToAngle(AERA.AERAValue_List[AERACount].AERA_Rotation.X),
                        Y = HTK_3DES.TSRSystem.RadianToAngle(AERA.AERAValue_List[AERACount].AERA_Rotation.Y),
                        Z = HTK_3DES.TSRSystem.RadianToAngle(AERA.AERAValue_List[AERACount].AERA_Rotation.Z)
                    },
                    Scales = new KMPPropertyGridSettings.AERA_Section.AERAValue.Scale
                    {
                        X = (float)AERA.AERAValue_List[AERACount].AERA_Scale.X,
                        Y = (float)AERA.AERAValue_List[AERACount].AERA_Scale.Y,
                        Z = (float)AERA.AERAValue_List[AERACount].AERA_Scale.Z
                    },
                    AERA_UnkByte1 = AERA.AERAValue_List[AERACount].AERA_UnkByte1,
                    AERA_UnkByte2 = AERA.AERAValue_List[AERACount].AERA_UnkByte2,
                    RouteID = AERA.AERAValue_List[AERACount].RouteID,
                    EnemyID = AERA.AERAValue_List[AERACount].EnemyID,
                    AERA_UnkByte4 = AERA.AERAValue_List[AERACount].AERA_UnkByte4
                };

                AERAValues_List.Add(AERAValue);

                #region Add Model(Area)
                HTK_3DES.TSRSystem.Transform_Value Area_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                {
                    Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                    {
                        X = AERA.AERAValue_List[AERACount].AERA_Position.X,
                        Y = AERA.AERAValue_List[AERACount].AERA_Position.Y,
                        Z = AERA.AERAValue_List[AERACount].AERA_Position.Z
                    },
                    Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                    {
                        X = AERA.AERAValue_List[AERACount].AERA_Scale.X * 1000,
                        Y = AERA.AERAValue_List[AERACount].AERA_Scale.Y * 1000,
                        Z = AERA.AERAValue_List[AERACount].AERA_Scale.Z * 1000
                    },
                    Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                    {
                        X = AERA.AERAValue_List[AERACount].AERA_Rotation.X,
                        Y = AERA.AERAValue_List[AERACount].AERA_Rotation.Y,
                        Z = AERA.AERAValue_List[AERACount].AERA_Rotation.Z
                    }
                };

                ModelVisual3D dv3D_AreaOBJ = null;
                if(AERAValue.AreaModeSettings.AreaModeValue == 0) dv3D_AreaOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Area\\Area_Box\\Area_Box.obj");
                if(AERAValue.AreaModeSettings.AreaModeValue == 1) dv3D_AreaOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Area\\Area_Cylinder\\Area_Cylinder.obj");
                if(AERAValue.AreaModeSettings.AreaModeValue > 1) dv3D_AreaOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Area\\Area_Box\\Area_Box.obj");

                //モデルの名前と番号を文字列に格納(情報化)
                HTK_3DES.TSRSystem.SetString_MV3D(dv3D_AreaOBJ, "Area " + AERACount + " " + -1);

                HTK_3DES.TransformMV3D.Transform_MV3D(Area_transform_Value, dv3D_AreaOBJ);

                //Area_MV3D_List.Add(dv3D_AreaOBJ);
                KMPViewportObject.Area_MV3DList.Add(dv3D_AreaOBJ);

                render.MainViewPort.Children.Add(dv3D_AreaOBJ);
                #endregion
            }

            AERA_Section.AERAValueList = AERAValues_List;
            #endregion

            #region Camera
            EMAC_Section = new KMPPropertyGridSettings.EMAC_Section
            {
                EMACValueList = null
            };

            List<KMPPropertyGridSettings.EMAC_Section.EMACValue> EMACValues_List = new List<KMPPropertyGridSettings.EMAC_Section.EMACValue>();

            for(int EMACCount = 0; EMACCount < EMAC.NumOfEntries; EMACCount++)
            {
                KMPPropertyGridSettings.EMAC_Section.EMACValue EMACValue = new KMPPropertyGridSettings.EMAC_Section.EMACValue
                {
                    ID = EMACCount,
                    CameraType = EMAC.EMACValue_List[EMACCount].CameraType,
                    NextCameraIndex = EMAC.EMACValue_List[EMACCount].NextCameraIndex,
                    EMAC_UnkBytes1 = EMAC.EMACValue_List[EMACCount].EMAC_UnkBytes1,
                    EMAC_ITOP_CameraIndex = EMAC.EMACValue_List[EMACCount].EMAC_ITOP_CameraIndex,
                    SpeedSettings = new KMPPropertyGridSettings.EMAC_Section.EMACValue.SpeedSetting
                    {
                        RouteSpeed = EMAC.EMACValue_List[EMACCount].RouteSpeed,
                        FOVSpeed = EMAC.EMACValue_List[EMACCount].FOVSpeed,
                        ViewpointSpeed = EMAC.EMACValue_List[EMACCount].ViewpointSpeed
                    },
                    EMAC_UnkBytes2 = EMAC.EMACValue_List[EMACCount].EMAC_UnkBytes2,
                    EMAC_UnkBytes3 = EMAC.EMACValue_List[EMACCount].EMAC_UnkBytes3,
                    Positions = new KMPPropertyGridSettings.EMAC_Section.EMACValue.Position
                    {
                        X = (float)EMAC.EMACValue_List[EMACCount].EMAC_Position.X,
                        Y = (float)EMAC.EMACValue_List[EMACCount].EMAC_Position.Y,
                        Z = (float)EMAC.EMACValue_List[EMACCount].EMAC_Position.Z
                    },
                    Rotations = new KMPPropertyGridSettings.EMAC_Section.EMACValue.Rotation
                    {
                        X = HTK_3DES.TSRSystem.RadianToAngle(EMAC.EMACValue_List[EMACCount].EMAC_Rotation.X),
                        Y = HTK_3DES.TSRSystem.RadianToAngle(EMAC.EMACValue_List[EMACCount].EMAC_Rotation.Y),
                        Z = HTK_3DES.TSRSystem.RadianToAngle(EMAC.EMACValue_List[EMACCount].EMAC_Rotation.Z)
                    },
                    FOVAngleSettings = new KMPPropertyGridSettings.EMAC_Section.EMACValue.FOVAngleSetting
                    {
                        FOVAngle_Start = EMAC.EMACValue_List[EMACCount].FOVAngle_Start,
                        FOVAngle_End = EMAC.EMACValue_List[EMACCount].FOVAngle_End
                    },
                    Viewpoint_Start = new KMPPropertyGridSettings.EMAC_Section.EMACValue.ViewpointStart
                    {
                        X = (float)EMAC.EMACValue_List[EMACCount].Viewpoint_Start.X,
                        Y = (float)EMAC.EMACValue_List[EMACCount].Viewpoint_Start.Y,
                        Z = (float)EMAC.EMACValue_List[EMACCount].Viewpoint_Start.Z
                    },
                    Viewpoint_Destination = new KMPPropertyGridSettings.EMAC_Section.EMACValue.ViewpointDestination
                    {
                        X = (float)EMAC.EMACValue_List[EMACCount].Viewpoint_Destination.X,
                        Y = (float)EMAC.EMACValue_List[EMACCount].Viewpoint_Destination.Y,
                        Z = (float)EMAC.EMACValue_List[EMACCount].Viewpoint_Destination.Z
                    },
                    Camera_Active_Time = EMAC.EMACValue_List[EMACCount].Camera_Active_Time
                };

                EMACValues_List.Add(EMACValue);

                #region Add Model(Camera)
                HTK_3DES.TSRSystem.Transform_Value Camera_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                {
                    Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                    {
                        X = EMAC.EMACValue_List[EMACCount].EMAC_Position.X,
                        Y = EMAC.EMACValue_List[EMACCount].EMAC_Position.Y,
                        Z = EMAC.EMACValue_List[EMACCount].EMAC_Position.Z
                    },
                    Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                    {
                        X = 10,
                        Y = 10,
                        Z = 10
                    },
                    Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                    {
                        X = EMAC.EMACValue_List[EMACCount].EMAC_Rotation.X,
                        Y = EMAC.EMACValue_List[EMACCount].EMAC_Rotation.Y,
                        Z = EMAC.EMACValue_List[EMACCount].EMAC_Rotation.Z
                    }
                };

                ModelVisual3D dv3D_CameraOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Camera\\Camera.obj");

                //モデルの名前と番号を文字列に格納(情報化)
                HTK_3DES.TSRSystem.SetString_MV3D(dv3D_CameraOBJ, "Camera " + EMACCount + " " + -1);

                HTK_3DES.TransformMV3D.Transform_MV3D(Camera_transform_Value, dv3D_CameraOBJ);

                //Camera_MV3D_List.Add(dv3D_CameraOBJ);
                KMPViewportObject.Camera_MV3DList.Add(dv3D_CameraOBJ);

                render.MainViewPort.Children.Add(dv3D_CameraOBJ);
                #endregion
            }

            EMAC_Section.EMACValueList = EMACValues_List;
            #endregion

            #region JugemPoint
            TPGJ_Section = new KMPPropertyGridSettings.TPGJ_Section
            {
                TPGJValueList = null
            };

            List<KMPPropertyGridSettings.TPGJ_Section.TPGJValue> TPGJValues_List = new List<KMPPropertyGridSettings.TPGJ_Section.TPGJValue>();

            for(int TPGJCount = 0; TPGJCount < TPGJ.NumOfEntries; TPGJCount++)
            {
                KMPPropertyGridSettings.TPGJ_Section.TPGJValue TPGJValue = new KMPPropertyGridSettings.TPGJ_Section.TPGJValue
                {
                    ID = TPGJCount,
                    TPGJ_RespawnID = TPGJ.TPGJValue_List[TPGJCount].TPGJ_RespawnID,
                    Positions = new KMPPropertyGridSettings.TPGJ_Section.TPGJValue.Position
                    {
                        X = (float)TPGJ.TPGJValue_List[TPGJCount].TPGJ_Position.X,
                        Y = (float)TPGJ.TPGJValue_List[TPGJCount].TPGJ_Position.Y,
                        Z = (float)TPGJ.TPGJValue_List[TPGJCount].TPGJ_Position.Z
                    },
                    Rotations = new KMPPropertyGridSettings.TPGJ_Section.TPGJValue.Rotation
                    {
                        X = HTK_3DES.TSRSystem.RadianToAngle(TPGJ.TPGJValue_List[TPGJCount].TPGJ_Rotation.X),
                        Y = HTK_3DES.TSRSystem.RadianToAngle(TPGJ.TPGJValue_List[TPGJCount].TPGJ_Rotation.Y),
                        Z = HTK_3DES.TSRSystem.RadianToAngle(TPGJ.TPGJValue_List[TPGJCount].TPGJ_Rotation.Z)
                    },
                    TPGJ_UnkBytes1 = TPGJ.TPGJValue_List[TPGJCount].TPGJ_UnkBytes1
                };

                TPGJValues_List.Add(TPGJValue);

                #region Add Model(RespawnPoint)
                HTK_3DES.TSRSystem.Transform_Value RespawnPoint_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                {
                    Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                    {
                        X = TPGJ.TPGJValue_List[TPGJCount].TPGJ_Position.X,
                        Y = TPGJ.TPGJValue_List[TPGJCount].TPGJ_Position.Y,
                        Z = TPGJ.TPGJValue_List[TPGJCount].TPGJ_Position.Z
                    },
                    Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                    {
                        X = 10,
                        Y = 10,
                        Z = 10
                    },
                    Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                    {
                        X = TPGJ.TPGJValue_List[TPGJCount].TPGJ_Rotation.X,
                        Y = TPGJ.TPGJValue_List[TPGJCount].TPGJ_Rotation.Y,
                        Z = TPGJ.TPGJValue_List[TPGJCount].TPGJ_Rotation.Z
                    }
                };

                ModelVisual3D dv3D_RespawnPointOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\RespawnPoint\\RespawnPoint.obj");

                //モデルの名前と番号を文字列に格納(情報化)
                HTK_3DES.TSRSystem.SetString_MV3D(dv3D_RespawnPointOBJ, "RespawnPoint " + TPGJCount + " " + -1);

                HTK_3DES.TransformMV3D.Transform_MV3D(RespawnPoint_transform_Value, dv3D_RespawnPointOBJ);

                //RespawnPoint_MV3D_List.Add(dv3D_RespawnPointOBJ);
                KMPViewportObject.RespawnPoint_MV3DList.Add(dv3D_RespawnPointOBJ);

                render.MainViewPort.Children.Add(dv3D_RespawnPointOBJ);
                #endregion
            }

            TPGJ_Section.TPGJValueList = TPGJValues_List;
            #endregion

            //TPNC : Unused Section
            //TPSM : Unused Section

            #region StageInfo
            IGTS_Section = new KMPPropertyGridSettings.IGTS_Section
            {
                Unknown1 = IGTS.Unknown1,
                LapCount = IGTS.LapCount,
                PolePosition = IGTS.PolePosition,
                Unknown2 = IGTS.Unknown2,
                Unknown3 = IGTS.Unknown3,
                RGBAColor = new KMPPropertyGridSettings.IGTS_Section.RGBA
                {
                    R = IGTS.RGBAColor.R,
                    G = IGTS.RGBAColor.G,
                    B = IGTS.RGBAColor.B,
                    A = IGTS.RGBAColor.A
                },
                FlareAlpha = IGTS.FlareAlpha
            };
            #endregion

            //SROC : Unused Section

            #region GlideRoute
            HPLG_TPLG_Section = new KMPPropertyGridSettings.HPLG_TPLG_Section
            {
                HPLGValueList = null
            };

            List<KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue> HPLGValues_List = new List<KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue>();

            for (int i = 0; i < HPLG.NumOfEntries; i++)
            {
                //Rail
                HTK_3DES.PathTools.Rail GlideRoute_Rail = new HTK_3DES.PathTools.Rail
                {
                    TV3D_List = new List<TubeVisual3D>(),
                    MV3D_List = new List<ModelVisual3D>()
                };

                KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue HPLGValue = new KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue
                {
                    GroupID = i,
                    HPLG_PreviewGroup = new KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.HPLG_PreviewGroups
                    {
                        Prev0 = HPLG.HPLGValue_List[i].HPLG_PreviewGroup.Prev0,
                        Prev1 = HPLG.HPLGValue_List[i].HPLG_PreviewGroup.Prev1,
                        Prev2 = HPLG.HPLGValue_List[i].HPLG_PreviewGroup.Prev2,
                        Prev3 = HPLG.HPLGValue_List[i].HPLG_PreviewGroup.Prev3,
                        Prev4 = HPLG.HPLGValue_List[i].HPLG_PreviewGroup.Prev4,
                        Prev5 = HPLG.HPLGValue_List[i].HPLG_PreviewGroup.Prev5
                    },
                    HPLG_NextGroup = new KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.HPLG_NextGroups
                    {
                        Next0 = HPLG.HPLGValue_List[i].HPLG_NextGroup.Next0,
                        Next1 = HPLG.HPLGValue_List[i].HPLG_NextGroup.Next1,
                        Next2 = HPLG.HPLGValue_List[i].HPLG_NextGroup.Next2,
                        Next3 = HPLG.HPLGValue_List[i].HPLG_NextGroup.Next3,
                        Next4 = HPLG.HPLGValue_List[i].HPLG_NextGroup.Next4,
                        Next5 = HPLG.HPLGValue_List[i].HPLG_NextGroup.Next5
                    },
                    RouteSettings = new KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.RouteSetting
                    {
                        ForceToRoute = GlideRouteFlagConverter.ConvertFlags(HPLG.HPLGValue_List[i].RouteSetting, KMPs.KMPHelper.FlagConverter.GlideRoute.FlagType.ForceToRoute),
                        CannonSection = GlideRouteFlagConverter.ConvertFlags(HPLG.HPLGValue_List[i].RouteSetting, KMPs.KMPHelper.FlagConverter.GlideRoute.FlagType.CannonSection),
                        PreventRaising = GlideRouteFlagConverter.ConvertFlags(HPLG.HPLGValue_List[i].RouteSetting, KMPs.KMPHelper.FlagConverter.GlideRoute.FlagType.PreventRaising),
                    },
                    HPLG_UnkBytes2 = HPLG.HPLGValue_List[i].HPLG_UnkBytes2,
                    TPLGValueList = null
                };

                List<KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.TPLGValue> TPLGValues_List = new List<KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.TPLGValue>();

                for (int Count = 0; Count < HPLG.HPLGValue_List[i].HPLG_Length; Count++)
                {
                    KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.TPLGValue TPLGValue = new KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.TPLGValue
                    {
                        GroupID = i,
                        ID = Count,
                        Positions = new KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.TPLGValue.Position
                        {
                            X = (float)TPLG.TPLGValue_List[Count + HPLG.HPLGValue_List[i].HPLG_StartPoint].TPLG_Position.X,
                            Y = (float)TPLG.TPLGValue_List[Count + HPLG.HPLGValue_List[i].HPLG_StartPoint].TPLG_Position.Y,
                            Z = (float)TPLG.TPLGValue_List[Count + HPLG.HPLGValue_List[i].HPLG_StartPoint].TPLG_Position.Z
                        },
                        TPLG_PointScaleValue = TPLG.TPLGValue_List[Count + HPLG.HPLGValue_List[i].HPLG_StartPoint].TPLG_PointScaleValue,
                        TPLG_UnkBytes1 = TPLG.TPLGValue_List[Count + HPLG.HPLGValue_List[i].HPLG_StartPoint].TPLG_UnkBytes1,
                        TPLG_UnkBytes2 = TPLG.TPLGValue_List[Count + HPLG.HPLGValue_List[i].HPLG_StartPoint].TPLG_UnkBytes2
                    };

                    TPLGValues_List.Add(TPLGValue);

                    #region Add Model(GlideRoutes)
                    HTK_3DES.TSRSystem.Transform_Value GliderPoint_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                    {
                        Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                        {
                            X = TPLG.TPLGValue_List[Count + HPLG.HPLGValue_List[i].HPLG_StartPoint].TPLG_Position.X,
                            Y = TPLG.TPLGValue_List[Count + HPLG.HPLGValue_List[i].HPLG_StartPoint].TPLG_Position.Y,
                            Z = TPLG.TPLGValue_List[Count + HPLG.HPLGValue_List[i].HPLG_StartPoint].TPLG_Position.Z
                        },
                        Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                        {
                            X = TPLG.TPLGValue_List[Count + HPLG.HPLGValue_List[i].HPLG_StartPoint].TPLG_PointScaleValue * 100,
                            Y = TPLG.TPLGValue_List[Count + HPLG.HPLGValue_List[i].HPLG_StartPoint].TPLG_PointScaleValue * 100,
                            Z = TPLG.TPLGValue_List[Count + HPLG.HPLGValue_List[i].HPLG_StartPoint].TPLG_PointScaleValue * 100
                        },
                        Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        }
                    };

                    ModelVisual3D dv3D_GliderPathOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\GliderPath\\GliderPath.obj");

                    //モデルの名前と番号を文字列に格納(情報化)
                    HTK_3DES.TSRSystem.SetString_MV3D(dv3D_GliderPathOBJ, "GlideRoutes " + Count + " " + i);

                    HTK_3DES.TransformMV3D.Transform_MV3D(GliderPoint_transform_Value, dv3D_GliderPathOBJ);

                    //Add model
                    GlideRoute_Rail.MV3D_List.Add(dv3D_GliderPathOBJ);

                    render.MainViewPort.Children.Add(dv3D_GliderPathOBJ);
                    #endregion
                }

                HPLGValue.TPLGValueList = TPLGValues_List;

                HPLGValues_List.Add(HPLGValue);

                KMPViewportObject.GlideRoute_Rail_List.Add(GlideRoute_Rail);
            }

            HPLG_TPLG_Section.HPLGValueList = HPLGValues_List;

            #region Add Rail
            for (int i = 0; i < KMPViewportObject.GlideRoute_Rail_List.Count; i++)
            {
                List<Point3D> point3Ds = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.GlideRoute_Rail_List[i].MV3D_List);
                KMPViewportObject.GlideRoute_Rail_List[i].TV3D_List = HTK_3DES.PathTools.DrawPath_Tube(render, point3Ds, 10.0, Colors.LightSkyBlue);
            }
            #endregion

            #endregion

            #endregion

            br1.Close();
            fs1.Close();

            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Area.Checked, render, KMPViewportObject.StartPosition_MV3DList);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_EnemyRoutes.Checked, render, KMPViewportObject.EnemyRoute_Rail_List);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_ItemRoutes.Checked, render, KMPViewportObject.ItemRoute_Rail_List);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Checkpoint.Checked, render, KMPViewportObject.Checkpoint_Rail);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_OBJ.Checked, render, KMPViewportObject.OBJ_MV3DList);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Routes.Checked, render, KMPViewportObject.Routes_List);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Area.Checked, render, KMPViewportObject.Area_MV3DList);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Camera.Checked, render, KMPViewportObject.Camera_MV3DList);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Returnpoints.Checked, render, KMPViewportObject.RespawnPoint_MV3DList);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_GlideRoutes.Checked, render, KMPViewportObject.GlideRoute_Rail_List);

            if (KMPSectionComboBox.Items.Count == 0)
            {
                string[] AllSectionAry = new string[] { "KartPoint", "EnemyRoutes", "ItemRoutes", "CheckPoint", "Obj", "Route", "Area", "Camera", "JugemPoint", "GlideRoutes" };
                KMPSectionComboBox.Items.AddRange(AllSectionAry.ToArray());
            }

            KMPSectionComboBox.SelectedIndex = 0;

            if (IGTS_Section != null)
            {
                //Display only IGTS section directly to PropertyGrid
                propertyGrid_KMP_StageInfo.SelectedObject = IGTS_Section;
            }

            writeBinaryToolStripMenuItem.Enabled = true;
            closeKMPToolStripMenuItem.Enabled = true;
            exportToolStripMenuItem.Enabled = true;
            inputXmlAsXXXXToolStripMenuItem.Enabled = true;
        }

        private void writeBinaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog Save_KMP = new SaveFileDialog()
            {
                Title = "Save KMP",
                InitialDirectory = @"C:\Users\User\Desktop",
                Filter = "kmp file|*.kmp"
            };

            if (Save_KMP.ShowDialog() != DialogResult.OK) return;

            System.IO.FileStream fs1 = new FileStream(Save_KMP.FileName, FileMode.Create, FileAccess.Write);
            BinaryWriter bw1 = new BinaryWriter(fs1);

            KMPs.KMPWriter.TPNE_HPNE_WritePosition tPNE_HPNE_WritePosition = new KMPs.KMPWriter.TPNE_HPNE_WritePosition();
            KMPs.KMPWriter.TPTI_HPTI_WritePosition tPTI_HPTI_WritePosition = new KMPs.KMPWriter.TPTI_HPTI_WritePosition();
            KMPs.KMPWriter.TPKC_HPKC_WritePosition tPKC_HPKC_WritePosition = new KMPs.KMPWriter.TPKC_HPKC_WritePosition();
            KMPs.KMPWriter.TPLG_HPLG_WritePosition tPLG_HPLG_WritePosition = new KMPs.KMPWriter.TPLG_HPLG_WritePosition();

            long pos = bw1.BaseStream.Position;

            #region Temp
            KMPs.KMPFormat KMPFormat_Temp = new KMPs.KMPFormat
            {
                DMDCHeader = new char[] { ' ', ' ', ' ', ' ' },
                FileSize = 0,
                SectionCount = 0,
                DMDCHeaderSize = 0,
                VersionNumber = 0,
                DMDC_SectionOffset = new KMPs.KMPFormat.DMDCSectionOffset
                {
                    TPTK_Offset = 0,
                    TPNE_Offset = 0,
                    HPNE_Offset = 0,
                    TPTI_Offset = 0,
                    HPTI_Offset = 0,
                    TPKC_Offset = 0,
                    HPKC_Offset = 0,
                    JBOG_Offset = 0,
                    ITOP_Offset = 0,
                    AERA_Offset = 0,
                    EMAC_Offset = 0,
                    TPGJ_Offset = 0,
                    TPNC_Offset = 0,
                    TPSM_Offset = 0,
                    IGTS_Offset = 0,
                    SROC_Offset = 0,
                    TPLG_Offset = 0,
                    HPLG_Offset = 0
                }
                //KMP_Section = null
            };

            bw1.Write(KMPFormat_Temp.DMDCHeader);
            bw1.Write(KMPFormat_Temp.FileSize);
            bw1.Write(KMPFormat_Temp.SectionCount);
            bw1.Write(KMPFormat_Temp.DMDCHeaderSize);
            bw1.Write(KMPFormat_Temp.VersionNumber);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.TPTK_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.TPNE_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.HPNE_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.TPTI_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.HPTI_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.TPKC_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.HPKC_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.JBOG_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.ITOP_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.AERA_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.EMAC_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.TPGJ_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.TPNC_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.TPSM_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.IGTS_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.SROC_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.TPLG_Offset);
            bw1.Write(KMPFormat_Temp.DMDC_SectionOffset.HPLG_Offset);
            #endregion

            #region TPTK
            KMPs.KMPFormat.KMPSection.TPTK_Section TPTK = new KMPs.KMPFormat.KMPSection.TPTK_Section
            {
                TPTKHeader = new char[] { 'T', 'P', 'T', 'K' },
                NumOfEntries = Convert.ToUInt16(TPTK_Section.TPTKValueList.Count),
                AdditionalValue = 0,
                TPTKValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.TPTK_Section.TPTKValue> TPTK_Value_List = new List<KMPs.KMPFormat.KMPSection.TPTK_Section.TPTKValue>();

            for (int Count = 0; Count < TPTK_Section.TPTKValueList.Count; Count++)
            {
                #region Transform
                double PX = TPTK_Section.TPTKValueList[Count].Position_Value.X;
                double PY = TPTK_Section.TPTKValueList[Count].Position_Value.Y;
                double PZ = TPTK_Section.TPTKValueList[Count].Position_Value.Z;

                double RX = HTK_3DES.TSRSystem.AngleToRadian(TPTK_Section.TPTKValueList[Count].Rotate_Value.X);
                double RY = HTK_3DES.TSRSystem.AngleToRadian(TPTK_Section.TPTKValueList[Count].Rotate_Value.Y);
                double RZ = HTK_3DES.TSRSystem.AngleToRadian(TPTK_Section.TPTKValueList[Count].Rotate_Value.Z);
                #endregion

                KMPs.KMPFormat.KMPSection.TPTK_Section.TPTKValue TPTK_Values = new KMPs.KMPFormat.KMPSection.TPTK_Section.TPTKValue
                {
                    TPTK_Position = new Vector3D(PX, PY, PZ),
                    TPTK_Rotation = new Vector3D(RX, RY, RZ),
                    Player_Index = Convert.ToUInt16(TPTK_Section.TPTKValueList[Count].Player_Index),
                    TPTK_UnkBytes = Convert.ToUInt16(TPTK_Section.TPTKValueList[Count].TPTK_UnkBytes)
                };

                TPTK_Value_List.Add(TPTK_Values);
            }

            TPTK.TPTKValue_List = TPTK_Value_List;

            uint TPTKPos = KMPs.KMPWriter.Write_TPTK(bw1, TPTK);
            #endregion

            if (HPNE_TPNE_Section.HPNEValueList.Count != 0)
            {
                List<KMPs.KMPFormat.KMPSection.TPNE_Section.TPNEValue> TPNE_Values_List = new List<KMPs.KMPFormat.KMPSection.TPNE_Section.TPNEValue>();
                List<KMPs.KMPFormat.KMPSection.HPNE_Section.HPNEValue> HPNE_Values_List = new List<KMPs.KMPFormat.KMPSection.HPNE_Section.HPNEValue>();

                int StartPoint = 0;
                for (int HPNECount = 0; HPNECount < HPNE_TPNE_Section.HPNEValueList.Count; HPNECount++)
                {
                    KMPs.KMPFormat.KMPSection.HPNE_Section.HPNEValue HPNE_Values = new KMPs.KMPFormat.KMPSection.HPNE_Section.HPNEValue
                    {
                        HPNE_StartPoint = Convert.ToUInt16(StartPoint),
                        HPNE_Length = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].TPNEValueList.Count),
                        HPNE_PreviewGroup = new KMPs.KMPFormat.KMPSection.HPNE_Section.HPNEValue.HPNE_PreviewGroups
                        {
                            Prev0 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNEPreviewGroups.Prev0),
                            Prev1 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNEPreviewGroups.Prev1),
                            Prev2 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNEPreviewGroups.Prev2),
                            Prev3 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNEPreviewGroups.Prev3),
                            Prev4 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNEPreviewGroups.Prev4),
                            Prev5 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNEPreviewGroups.Prev5),
                            Prev6 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNEPreviewGroups.Prev6),
                            Prev7 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNEPreviewGroups.Prev7),
                            Prev8 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNEPreviewGroups.Prev8),
                            Prev9 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNEPreviewGroups.Prev9),
                            Prev10 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNEPreviewGroups.Prev10),
                            Prev11 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNEPreviewGroups.Prev11),
                            Prev12 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNEPreviewGroups.Prev12),
                            Prev13 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNEPreviewGroups.Prev13),
                            Prev14 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNEPreviewGroups.Prev14),
                            Prev15 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNEPreviewGroups.Prev15)
                        },
                        HPNE_NextGroup = new KMPs.KMPFormat.KMPSection.HPNE_Section.HPNEValue.HPNE_NextGroups
                        {
                            Next0 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNENextGroups.Next0),
                            Next1 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNENextGroups.Next1),
                            Next2 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNENextGroups.Next2),
                            Next3 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNENextGroups.Next3),
                            Next4 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNENextGroups.Next4),
                            Next5 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNENextGroups.Next5),
                            Next6 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNENextGroups.Next6),
                            Next7 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNENextGroups.Next7),
                            Next8 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNENextGroups.Next8),
                            Next9 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNENextGroups.Next9),
                            Next10 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNENextGroups.Next10),
                            Next11 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNENextGroups.Next11),
                            Next12 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNENextGroups.Next12),
                            Next13 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNENextGroups.Next13),
                            Next14 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNENextGroups.Next14),
                            Next15 = Convert.ToUInt16(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNENextGroups.Next15)
                        },
                        HPNE_UnkBytes1 = Convert.ToUInt32(HPNE_TPNE_Section.HPNEValueList[HPNECount].HPNE_UnkBytes1)
                    };

                    HPNE_Values_List.Add(HPNE_Values);

                    for(int TPNECount = 0; TPNECount < HPNE_TPNE_Section.HPNEValueList[HPNECount].TPNEValueList.Count; TPNECount++)
                    {
                        double PX = HPNE_TPNE_Section.HPNEValueList[HPNECount].TPNEValueList[TPNECount].Positions.X;
                        double PY = HPNE_TPNE_Section.HPNEValueList[HPNECount].TPNEValueList[TPNECount].Positions.Y;
                        double PZ = HPNE_TPNE_Section.HPNEValueList[HPNECount].TPNEValueList[TPNECount].Positions.Z;

                        KMPs.KMPFormat.KMPSection.TPNE_Section.TPNEValue TPNE_Values = new KMPs.KMPFormat.KMPSection.TPNE_Section.TPNEValue
                        {
                            TPNE_Position = new Vector3D(PX, PY, PZ),
                            Control = Convert.ToSingle(HPNE_TPNE_Section.HPNEValueList[HPNECount].TPNEValueList[TPNECount].Control),
                            MushSetting = HPNE_TPNE_Section.HPNEValueList[HPNECount].TPNEValueList[TPNECount].MushSettings.MushSettingValue,
                            DriftSetting = Convert.ToByte(HPNE_TPNE_Section.HPNEValueList[HPNECount].TPNEValueList[TPNECount].DriftSettings.DriftSettingValue),
                            Flags = Convert.ToByte(HPNE_TPNE_Section.HPNEValueList[HPNECount].TPNEValueList[TPNECount].FlagSettings.Flags),
                            PathFindOption = HPNE_TPNE_Section.HPNEValueList[HPNECount].TPNEValueList[TPNECount].PathFindOptions.PathFindOptionValue,
                            MaxSearchYOffset = HPNE_TPNE_Section.HPNEValueList[HPNECount].TPNEValueList[TPNECount].MaxSearchYOffset.MaxSearchYOffsetValue
                        };

                        TPNE_Values_List.Add(TPNE_Values);

                        StartPoint++;
                    }
                }

                KMPs.KMPFormat.KMPSection.TPNE_Section TPNE = new KMPs.KMPFormat.KMPSection.TPNE_Section
                {
                    TPNEHeader = new char[] { 'T', 'P', 'N', 'E' },
                    NumOfEntries = Convert.ToUInt16(TPNE_Values_List.Count),
                    AdditionalValue = 0,
                    TPNEValue_List = TPNE_Values_List
                };

                KMPs.KMPFormat.KMPSection.HPNE_Section HPNE = new KMPs.KMPFormat.KMPSection.HPNE_Section
                {
                    HPNEHeader = new char[] { 'H', 'P', 'N', 'E' },
                    NumOfEntries = Convert.ToUInt16(HPNE_Values_List.Count),
                    AdditionalValue = 0,
                    HPNEValue_List = HPNE_Values_List
                };

                tPNE_HPNE_WritePosition = KMPs.KMPWriter.Write_TPNE_HPNE(bw1, TPNE, HPNE);
            }
            if (HPNE_TPNE_Section.HPNEValueList.Count == 0)
            {
                KMPs.KMPFormat.KMPSection.TPNE_Section TPNE = new KMPs.KMPFormat.KMPSection.TPNE_Section
                {
                    TPNEHeader = new char[] { 'T', 'P', 'N', 'E' },
                    NumOfEntries = 0,
                    AdditionalValue = 0,
                    TPNEValue_List = new List<KMPs.KMPFormat.KMPSection.TPNE_Section.TPNEValue>()
                };

                KMPs.KMPFormat.KMPSection.HPNE_Section HPNE = new KMPs.KMPFormat.KMPSection.HPNE_Section
                {
                    HPNEHeader = new char[] { 'H', 'P', 'N', 'E' },
                    NumOfEntries = 0,
                    AdditionalValue = 0,
                    HPNEValue_List = new List<KMPs.KMPFormat.KMPSection.HPNE_Section.HPNEValue>()
                };

                tPNE_HPNE_WritePosition = KMPs.KMPWriter.Write_TPNE_HPNE(bw1, TPNE, HPNE);
            }
            if (HPTI_TPTI_Section.HPTIValueList.Count != 0)
            {
                List<KMPs.KMPFormat.KMPSection.TPTI_Section.TPTIValue> TPTI_Values_List = new List<KMPs.KMPFormat.KMPSection.TPTI_Section.TPTIValue>();
                List<KMPs.KMPFormat.KMPSection.HPTI_Section.HPTIValue> HPTI_Values_List = new List<KMPs.KMPFormat.KMPSection.HPTI_Section.HPTIValue>();

                int StartPoint = 0;
                for (int HPTICount = 0; HPTICount < HPTI_TPTI_Section.HPTIValueList.Count; HPTICount++)
                {
                    KMPs.KMPFormat.KMPSection.HPTI_Section.HPTIValue HPTI_Values = new KMPs.KMPFormat.KMPSection.HPTI_Section.HPTIValue
                    {
                        HPTI_StartPoint = Convert.ToUInt16(StartPoint),
                        HPTI_Length = Convert.ToUInt16(HPTI_TPTI_Section.HPTIValueList[HPTICount].TPTIValueList.Count),
                        HPTI_PreviewGroup = new KMPs.KMPFormat.KMPSection.HPTI_Section.HPTIValue.HPTI_PreviewGroups
                        {
                            Prev0 = Convert.ToUInt16(HPTI_TPTI_Section.HPTIValueList[HPTICount].HPTI_PreviewGroup.Prev0),
                            Prev1 = Convert.ToUInt16(HPTI_TPTI_Section.HPTIValueList[HPTICount].HPTI_PreviewGroup.Prev1),
                            Prev2 = Convert.ToUInt16(HPTI_TPTI_Section.HPTIValueList[HPTICount].HPTI_PreviewGroup.Prev2),
                            Prev3 = Convert.ToUInt16(HPTI_TPTI_Section.HPTIValueList[HPTICount].HPTI_PreviewGroup.Prev3),
                            Prev4 = Convert.ToUInt16(HPTI_TPTI_Section.HPTIValueList[HPTICount].HPTI_PreviewGroup.Prev4),
                            Prev5 = Convert.ToUInt16(HPTI_TPTI_Section.HPTIValueList[HPTICount].HPTI_PreviewGroup.Prev5),
                        },
                        HPTI_NextGroup = new KMPs.KMPFormat.KMPSection.HPTI_Section.HPTIValue.HPTI_NextGroups
                        {
                            Next0 = Convert.ToUInt16(HPTI_TPTI_Section.HPTIValueList[HPTICount].HPTI_NextGroup.Next0),
                            Next1 = Convert.ToUInt16(HPTI_TPTI_Section.HPTIValueList[HPTICount].HPTI_NextGroup.Next1),
                            Next2 = Convert.ToUInt16(HPTI_TPTI_Section.HPTIValueList[HPTICount].HPTI_NextGroup.Next2),
                            Next3 = Convert.ToUInt16(HPTI_TPTI_Section.HPTIValueList[HPTICount].HPTI_NextGroup.Next3),
                            Next4 = Convert.ToUInt16(HPTI_TPTI_Section.HPTIValueList[HPTICount].HPTI_NextGroup.Next4),
                            Next5 = Convert.ToUInt16(HPTI_TPTI_Section.HPTIValueList[HPTICount].HPTI_NextGroup.Next5),
                        }
                    };

                    HPTI_Values_List.Add(HPTI_Values);

                    for(int TPTICount = 0; TPTICount < HPTI_TPTI_Section.HPTIValueList[HPTICount].TPTIValueList.Count; TPTICount++)
                    {
                        double PX = HPTI_TPTI_Section.HPTIValueList[HPTICount].TPTIValueList[TPTICount].TPTI_Positions.X;
                        double PY = HPTI_TPTI_Section.HPTIValueList[HPTICount].TPTIValueList[TPTICount].TPTI_Positions.Y;
                        double PZ = HPTI_TPTI_Section.HPTIValueList[HPTICount].TPTIValueList[TPTICount].TPTI_Positions.Z;

                        KMPs.KMPFormat.KMPSection.TPTI_Section.TPTIValue TPTI_Values = new KMPs.KMPFormat.KMPSection.TPTI_Section.TPTIValue
                        {
                            TPTI_Position = new Vector3D(PX, PY, PZ),
                            TPTI_PointSize = Convert.ToSingle(HPTI_TPTI_Section.HPTIValueList[HPTICount].TPTIValueList[TPTICount].TPTI_PointSize),
                            GravityMode = HPTI_TPTI_Section.HPTIValueList[HPTICount].TPTIValueList[TPTICount].GravityModeSettings.GravityModeValue,
                            PlayerScanRadius = HPTI_TPTI_Section.HPTIValueList[HPTICount].TPTIValueList[TPTICount].PlayerScanRadiusSettings.PlayerScanRadiusValue
                        };

                        TPTI_Values_List.Add(TPTI_Values);

                        StartPoint++;
                    }
                }

                KMPs.KMPFormat.KMPSection.TPTI_Section TPTI = new KMPs.KMPFormat.KMPSection.TPTI_Section
                {
                    TPTIHeader = new char[] { 'T', 'P', 'T', 'I' },
                    NumOfEntries = Convert.ToUInt16(TPTI_Values_List.Count),
                    AdditionalValue = 0,
                    TPTIValue_List = TPTI_Values_List
                };

                KMPs.KMPFormat.KMPSection.HPTI_Section HPTI = new KMPs.KMPFormat.KMPSection.HPTI_Section
                {
                    HPTIHeader = new char[] { 'H', 'P', 'T', 'I' },
                    NumOfEntries = Convert.ToUInt16(HPTI_Values_List.Count),
                    AdditionalValue = 0,
                    HPTIValue_List = HPTI_Values_List
                };

                tPTI_HPTI_WritePosition = KMPs.KMPWriter.Write_TPTI_HPTI(bw1, TPTI, HPTI);
            }
            if (HPTI_TPTI_Section.HPTIValueList.Count == 0)
            {
                KMPs.KMPFormat.KMPSection.TPTI_Section TPTI = new KMPs.KMPFormat.KMPSection.TPTI_Section
                {
                    TPTIHeader = new char[] { 'T', 'P', 'T', 'I' },
                    NumOfEntries = 0,
                    AdditionalValue = 0,
                    TPTIValue_List = new List<KMPs.KMPFormat.KMPSection.TPTI_Section.TPTIValue>()
                };

                KMPs.KMPFormat.KMPSection.HPTI_Section HPTI = new KMPs.KMPFormat.KMPSection.HPTI_Section
                {
                    HPTIHeader = new char[] { 'H', 'P', 'T', 'I' },
                    NumOfEntries = 0,
                    AdditionalValue = 0,
                    HPTIValue_List = new List<KMPs.KMPFormat.KMPSection.HPTI_Section.HPTIValue>()
                };

                tPTI_HPTI_WritePosition = KMPs.KMPWriter.Write_TPTI_HPTI(bw1, TPTI, HPTI);
            }
            if (HPKC_TPKC_Section.HPKCValueList.Count != 0)
            {
                List<KMPs.KMPFormat.KMPSection.TPKC_Section.TPKCValue> TPKC_Values_List = new List<KMPs.KMPFormat.KMPSection.TPKC_Section.TPKCValue>();
                List<KMPs.KMPFormat.KMPSection.HPKC_Section.HPKCValue> HPKC_Values_List = new List<KMPs.KMPFormat.KMPSection.HPKC_Section.HPKCValue>();

                int StartPoint = 0;
                for (int HPKCCount = 0; HPKCCount < HPKC_TPKC_Section.HPKCValueList.Count; HPKCCount++)
                {
                    KMPs.KMPFormat.KMPSection.HPKC_Section.HPKCValue HPKC_Values = new KMPs.KMPFormat.KMPSection.HPKC_Section.HPKCValue
                    {
                        HPKC_StartPoint = Convert.ToByte(StartPoint),
                        HPKC_Length = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].TPKCValueList.Count),
                        HPKC_PreviewGroup = new KMPs.KMPFormat.KMPSection.HPKC_Section.HPKCValue.HPKC_PreviewGroups
                        {
                            Prev0 = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].HPKC_PreviewGroup.Prev0),
                            Prev1 = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].HPKC_PreviewGroup.Prev1),
                            Prev2 = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].HPKC_PreviewGroup.Prev2),
                            Prev3 = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].HPKC_PreviewGroup.Prev3),
                            Prev4 = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].HPKC_PreviewGroup.Prev4),
                            Prev5 = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].HPKC_PreviewGroup.Prev5),
                        },
                        HPKC_NextGroup = new KMPs.KMPFormat.KMPSection.HPKC_Section.HPKCValue.HPKC_NextGroups
                        {
                            Next0 = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].HPKC_NextGroup.Next0),
                            Next1 = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].HPKC_NextGroup.Next1),
                            Next2 = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].HPKC_NextGroup.Next2),
                            Next3 = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].HPKC_NextGroup.Next3),
                            Next4 = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].HPKC_NextGroup.Next4),
                            Next5 = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].HPKC_NextGroup.Next5),
                        }
                    };

                    HPKC_Values_List.Add(HPKC_Values);

                    for(int TPKCCount = 0; TPKCCount < HPKC_TPKC_Section.HPKCValueList[HPKCCount].TPKCValueList.Count; TPKCCount++)
                    {
                        float PX_L = HPKC_TPKC_Section.HPKCValueList[HPKCCount].TPKCValueList[TPKCCount].Position_2D_Left.X;
                        float PY_L = HPKC_TPKC_Section.HPKCValueList[HPKCCount].TPKCValueList[TPKCCount].Position_2D_Left.Y;
                        float PX_R = HPKC_TPKC_Section.HPKCValueList[HPKCCount].TPKCValueList[TPKCCount].Position_2D_Right.X;
                        float PY_R = HPKC_TPKC_Section.HPKCValueList[HPKCCount].TPKCValueList[TPKCCount].Position_2D_Right.Y;

                        KMPs.KMPFormat.KMPSection.TPKC_Section.TPKCValue TPKC_Values = new KMPs.KMPFormat.KMPSection.TPKC_Section.TPKCValue
                        {
                            TPKC_2DPosition_Left = new Vector2(PX_L, PY_L),
                            TPKC_2DPosition_Right = new Vector2(PX_R, PY_R),
                            TPKC_RespawnID = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].TPKCValueList[TPKCCount].TPKC_RespawnID),
                            TPKC_Checkpoint_Type = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].TPKCValueList[TPKCCount].TPKC_Checkpoint_Type),
                            TPKC_PreviousCheckPoint = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].TPKCValueList[TPKCCount].TPKC_PreviousCheckPoint),
                            TPKC_NextCheckPoint = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].TPKCValueList[TPKCCount].TPKC_NextCheckPoint),
                            TPKC_UnkBytes1 = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].TPKCValueList[TPKCCount].TPKC_UnkBytes1),
                            TPKC_UnkBytes2 = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].TPKCValueList[TPKCCount].TPKC_UnkBytes2),
                            TPKC_UnkBytes3 = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].TPKCValueList[TPKCCount].TPKC_UnkBytes3),
                            TPKC_UnkBytes4 = Convert.ToByte(HPKC_TPKC_Section.HPKCValueList[HPKCCount].TPKCValueList[TPKCCount].TPKC_UnkBytes4)
                        };

                        TPKC_Values_List.Add(TPKC_Values);

                        StartPoint++;
                    }
                }

                KMPs.KMPFormat.KMPSection.TPKC_Section TPKC = new KMPs.KMPFormat.KMPSection.TPKC_Section
                {
                    TPKCHeader = new char[] { 'T', 'P', 'K', 'C' },
                    NumOfEntries = Convert.ToUInt16(TPKC_Values_List.Count),
                    AdditionalValue = 0,
                    TPKCValue_List = TPKC_Values_List
                };

                KMPs.KMPFormat.KMPSection.HPKC_Section HPKC = new KMPs.KMPFormat.KMPSection.HPKC_Section
                {
                    HPKCHeader = new char[] { 'H', 'P', 'K', 'C' },
                    NumOfEntries = Convert.ToUInt16(HPKC_Values_List.Count),
                    AdditionalValue = 0,
                    HPKCValue_List = HPKC_Values_List
                };

                tPKC_HPKC_WritePosition = KMPs.KMPWriter.Write_TPKC_HPKC(bw1, TPKC, HPKC);
            }
            if (HPKC_TPKC_Section.HPKCValueList.Count == 0)
            {
                KMPs.KMPFormat.KMPSection.TPKC_Section TPKC = new KMPs.KMPFormat.KMPSection.TPKC_Section
                {
                    TPKCHeader = new char[] { 'T', 'P', 'K', 'C' },
                    NumOfEntries = 0,
                    AdditionalValue = 0,
                    TPKCValue_List = new List<KMPs.KMPFormat.KMPSection.TPKC_Section.TPKCValue>()
                };

                KMPs.KMPFormat.KMPSection.HPKC_Section HPKC = new KMPs.KMPFormat.KMPSection.HPKC_Section
                {
                    HPKCHeader = new char[] { 'H', 'P', 'K', 'C' },
                    NumOfEntries = 0,
                    AdditionalValue = 0,
                    HPKCValue_List = new List<KMPs.KMPFormat.KMPSection.HPKC_Section.HPKCValue>()
                };

                tPKC_HPKC_WritePosition = KMPs.KMPWriter.Write_TPKC_HPKC(bw1, TPKC, HPKC);
            }

            #region JBOG
            KMPs.KMPFormat.KMPSection.JBOG_Section JBOG = new KMPs.KMPFormat.KMPSection.JBOG_Section
            {
                JBOGHeader = new char[] { 'J', 'B', 'O', 'G' },
                NumOfEntries = Convert.ToUInt16(JBOG_Section.JBOGValueList.Count),
                AdditionalValue = 0,
                JBOGValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.JBOG_Section.JBOGValue> JBOG_Value_List = new List<KMPs.KMPFormat.KMPSection.JBOG_Section.JBOGValue>();

            for (int Count = 0; Count < JBOG_Section.JBOGValueList.Count; Count++)
            {
                #region Transform Value
                double PX = JBOG_Section.JBOGValueList[Count].Positions.X;
                double PY = JBOG_Section.JBOGValueList[Count].Positions.Y;
                double PZ = JBOG_Section.JBOGValueList[Count].Positions.Z;

                double RX = HTK_3DES.TSRSystem.AngleToRadian(JBOG_Section.JBOGValueList[Count].Rotations.X);
                double RY = HTK_3DES.TSRSystem.AngleToRadian(JBOG_Section.JBOGValueList[Count].Rotations.Y);
                double RZ = HTK_3DES.TSRSystem.AngleToRadian(JBOG_Section.JBOGValueList[Count].Rotations.Z);

                double SX = JBOG_Section.JBOGValueList[Count].Scales.X;
                double SY = JBOG_Section.JBOGValueList[Count].Scales.Y;
                double SZ = JBOG_Section.JBOGValueList[Count].Scales.Z;
                #endregion

                KMPs.KMPFormat.KMPSection.JBOG_Section.JBOGValue JBOG_Values = new KMPs.KMPFormat.KMPSection.JBOG_Section.JBOGValue
                {
                    ObjectID = KMPs.KMPHelper.Byte2StringConverter.OBJIDStrToByteArray(JBOG_Section.JBOGValueList[Count].ObjectID),
                    JBOG_UnkByte1 = KMPs.KMPHelper.Byte2StringConverter.OBJIDStrToByteArray(JBOG_Section.JBOGValueList[Count].JBOG_UnkByte1),
                    JBOG_Position = new Vector3D(PX, PY, PZ),
                    JBOG_Rotation = new Vector3D(RX, RY, RZ),
                    JBOG_Scale = new Vector3D(SX, SY, SZ),
                    JBOG_ITOP_RouteIDIndex = Convert.ToUInt16(JBOG_Section.JBOGValueList[Count].JBOG_ITOP_RouteIDIndex),
                    JOBJ_Specific_Setting = new KMPs.KMPFormat.KMPSection.JBOG_Section.JBOGValue.JBOG_SpecificSetting
                    {
                        Value0 = Convert.ToUInt16(JBOG_Section.JBOGValueList[Count].JOBJ_Specific_Setting.Value0),
                        Value1 = Convert.ToUInt16(JBOG_Section.JBOGValueList[Count].JOBJ_Specific_Setting.Value1),
                        Value2 = Convert.ToUInt16(JBOG_Section.JBOGValueList[Count].JOBJ_Specific_Setting.Value2),
                        Value3 = Convert.ToUInt16(JBOG_Section.JBOGValueList[Count].JOBJ_Specific_Setting.Value3),
                        Value4 = Convert.ToUInt16(JBOG_Section.JBOGValueList[Count].JOBJ_Specific_Setting.Value4),
                        Value5 = Convert.ToUInt16(JBOG_Section.JBOGValueList[Count].JOBJ_Specific_Setting.Value5),
                        Value6 = Convert.ToUInt16(JBOG_Section.JBOGValueList[Count].JOBJ_Specific_Setting.Value6),
                        Value7 = Convert.ToUInt16(JBOG_Section.JBOGValueList[Count].JOBJ_Specific_Setting.Value7)
                    },
                    JBOG_PresenceSetting = Convert.ToUInt16(JBOG_Section.JBOGValueList[Count].JBOG_PresenceSetting),
                    JBOG_UnkByte2 = KMPs.KMPHelper.Byte2StringConverter.OBJIDStrToByteArray(JBOG_Section.JBOGValueList[Count].JBOG_UnkByte2),
                    JBOG_UnkByte3 = Convert.ToUInt16(JBOG_Section.JBOGValueList[Count].JBOG_UnkByte3)
                };

                JBOG_Value_List.Add(JBOG_Values);
            }

            JBOG.JBOGValue_List = JBOG_Value_List;

            uint ObjPos = KMPs.KMPWriter.Write_JBOG(bw1, JBOG);
            #endregion

            #region ITOP
            KMPs.KMPFormat.KMPSection.ITOP_Section ITOP = new KMPs.KMPFormat.KMPSection.ITOP_Section
            {
                ITOPHeader = new char[] { 'I', 'T', 'O', 'P' },
                ITOP_NumberOfRoute = Convert.ToUInt16(ITOP_Section.ITOP_RouteList.Count),
                ITOP_NumberOfPoint = Convert.ToUInt16(ITOP_Section.ITOP_RouteList.Select(x => x.ITOP_PointList.Count).Sum()),
                ITOP_Route_List = null
            };

            List<KMPs.KMPFormat.KMPSection.ITOP_Section.ITOP_Route> ITOP_Route_List = new List<KMPs.KMPFormat.KMPSection.ITOP_Section.ITOP_Route>();

            for (int ITOPRouteCount = 0; ITOPRouteCount < ITOP.ITOP_NumberOfRoute; ITOPRouteCount++)
            {
                KMPs.KMPFormat.KMPSection.ITOP_Section.ITOP_Route ITOP_Routes = new KMPs.KMPFormat.KMPSection.ITOP_Section.ITOP_Route
                {
                    ITOP_Route_NumOfPoint = Convert.ToUInt16(ITOP_Section.ITOP_RouteList[ITOPRouteCount].ITOP_PointList.Count),
                    ITOP_RouteSetting1 = Convert.ToByte(ITOP_Section.ITOP_RouteList[ITOPRouteCount].ITOP_RouteSetting1),
                    ITOP_RouteSetting2 = Convert.ToByte(ITOP_Section.ITOP_RouteList[ITOPRouteCount].ITOP_RouteSetting2),
                    ITOP_Point_List = null
                };

                List<KMPs.KMPFormat.KMPSection.ITOP_Section.ITOP_Route.ITOP_Point> ITOP_Point_List = new List<KMPs.KMPFormat.KMPSection.ITOP_Section.ITOP_Route.ITOP_Point>();

                for (int ITOP_PointCount = 0; ITOP_PointCount < ITOP_Routes.ITOP_Route_NumOfPoint; ITOP_PointCount++)
                {
                    double PX = ITOP_Section.ITOP_RouteList[ITOPRouteCount].ITOP_PointList[ITOP_PointCount].Positions.X;
                    double PY = ITOP_Section.ITOP_RouteList[ITOPRouteCount].ITOP_PointList[ITOP_PointCount].Positions.Y;
                    double PZ = ITOP_Section.ITOP_RouteList[ITOPRouteCount].ITOP_PointList[ITOP_PointCount].Positions.Z;

                    KMPs.KMPFormat.KMPSection.ITOP_Section.ITOP_Route.ITOP_Point ITOP_Points = new KMPs.KMPFormat.KMPSection.ITOP_Section.ITOP_Route.ITOP_Point
                    {
                        ITOP_Point_Position = new Vector3D(PX, PY, PZ),
                        ITOP_Point_RouteSpeed = Convert.ToUInt16(ITOP_Section.ITOP_RouteList[ITOPRouteCount].ITOP_PointList[ITOP_PointCount].ITOP_Point_RouteSpeed),
                        ITOP_PointSetting2 = Convert.ToUInt16(ITOP_Section.ITOP_RouteList[ITOPRouteCount].ITOP_PointList[ITOP_PointCount].ITOP_PointSetting2)
                    };

                    ITOP_Point_List.Add(ITOP_Points);
                }

                ITOP_Routes.ITOP_Point_List = ITOP_Point_List;

                ITOP_Route_List.Add(ITOP_Routes);
            }

            ITOP.ITOP_Route_List = ITOP_Route_List;

            uint RoutePos = KMPs.KMPWriter.Write_ITOP(bw1, ITOP);
            #endregion

            #region AERA
            KMPs.KMPFormat.KMPSection.AERA_Section AERA = new KMPs.KMPFormat.KMPSection.AERA_Section
            {
                AERAHeader = new char[] { 'A', 'E', 'R', 'A' },
                NumOfEntries = Convert.ToUInt16(AERA_Section.AERAValueList.Count),
                AdditionalValue = 0,
                AERAValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.AERA_Section.AERAValue> AERA_Value_List = new List<KMPs.KMPFormat.KMPSection.AERA_Section.AERAValue>();

            for (int Count = 0; Count < AERA_Section.AERAValueList.Count; Count++)
            {
                #region Transform
                double PX = AERA_Section.AERAValueList[Count].Positions.X;
                double PY = AERA_Section.AERAValueList[Count].Positions.Y;
                double PZ = AERA_Section.AERAValueList[Count].Positions.Z;

                double RX = HTK_3DES.TSRSystem.AngleToRadian(AERA_Section.AERAValueList[Count].Rotations.X);
                double RY = HTK_3DES.TSRSystem.AngleToRadian(AERA_Section.AERAValueList[Count].Rotations.Y);
                double RZ = HTK_3DES.TSRSystem.AngleToRadian(AERA_Section.AERAValueList[Count].Rotations.Z);

                double SX = AERA_Section.AERAValueList[Count].Scales.X;
                double SY = AERA_Section.AERAValueList[Count].Scales.Y;
                double SZ = AERA_Section.AERAValueList[Count].Scales.Z;
                #endregion

                KMPs.KMPFormat.KMPSection.AERA_Section.AERAValue AERA_Values = new KMPs.KMPFormat.KMPSection.AERA_Section.AERAValue
                {
                    AreaMode = Convert.ToByte(AERA_Section.AERAValueList[Count].AreaModeSettings.AreaModeValue),
                    AreaType = Convert.ToByte(AERA_Section.AERAValueList[Count].AreaType),
                    AERA_EMACIndex = Convert.ToByte(AERA_Section.AERAValueList[Count].AERA_EMACIndex),
                    Priority = Convert.ToByte(AERA_Section.AERAValueList[Count].Priority),
                    AERA_Position = new Vector3D(PX, PY, PZ),
                    AERA_Rotation = new Vector3D(RX, RY, RZ),
                    AERA_Scale = new Vector3D(SX, SY, SZ),
                    AERA_UnkByte1 = Convert.ToUInt16(AERA_Section.AERAValueList[Count].AERA_UnkByte1),
                    AERA_UnkByte2 = Convert.ToUInt16(AERA_Section.AERAValueList[Count].AERA_UnkByte2),
                    RouteID = Convert.ToByte(AERA_Section.AERAValueList[Count].RouteID),
                    EnemyID = Convert.ToByte(AERA_Section.AERAValueList[Count].EnemyID),
                    AERA_UnkByte4 = Convert.ToUInt16(AERA_Section.AERAValueList[Count].AERA_UnkByte4)
                };

                AERA_Value_List.Add(AERA_Values);
            }

            AERA.AERAValue_List = AERA_Value_List;

            uint AreaPos = KMPs.KMPWriter.Write_AERA(bw1, AERA);
            #endregion

            #region EMAC
            KMPs.KMPFormat.KMPSection.EMAC_Section EMAC = new KMPs.KMPFormat.KMPSection.EMAC_Section
            {
                EMACHeader = new char[] { 'E', 'M', 'A', 'C' },
                NumOfEntries = Convert.ToUInt16(EMAC_Section.EMACValueList.Count),
                AdditionalValue = 65535, //0xFFFF
                EMACValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.EMAC_Section.EMACValue> EMAC_Value_List = new List<KMPs.KMPFormat.KMPSection.EMAC_Section.EMACValue>();

            for (int EMACCount = 0; EMACCount < EMAC_Section.EMACValueList.Count; EMACCount++)
            {
                #region Transform Value
                double PX = EMAC_Section.EMACValueList[EMACCount].Positions.X;
                double PY = EMAC_Section.EMACValueList[EMACCount].Positions.Y;
                double PZ = EMAC_Section.EMACValueList[EMACCount].Positions.Z;

                double RX = HTK_3DES.TSRSystem.AngleToRadian(EMAC_Section.EMACValueList[EMACCount].Rotations.X);
                double RY = HTK_3DES.TSRSystem.AngleToRadian(EMAC_Section.EMACValueList[EMACCount].Rotations.Y);
                double RZ = HTK_3DES.TSRSystem.AngleToRadian(EMAC_Section.EMACValueList[EMACCount].Rotations.Z);
                #endregion

                #region Viewpoint Position(Start, End)
                double VP_Start_PX = EMAC_Section.EMACValueList[EMACCount].Viewpoint_Start.X;
                double VP_Start_PY = EMAC_Section.EMACValueList[EMACCount].Viewpoint_Start.Y;
                double VP_Start_PZ = EMAC_Section.EMACValueList[EMACCount].Viewpoint_Start.Z;

                double VP_Destination_PX = EMAC_Section.EMACValueList[EMACCount].Viewpoint_Destination.X;
                double VP_Destination_PY = EMAC_Section.EMACValueList[EMACCount].Viewpoint_Destination.Y;
                double VP_Destination_PZ = EMAC_Section.EMACValueList[EMACCount].Viewpoint_Destination.Z;
                #endregion

                KMPs.KMPFormat.KMPSection.EMAC_Section.EMACValue EMAC_Values = new KMPs.KMPFormat.KMPSection.EMAC_Section.EMACValue
                {
                    CameraType = Convert.ToByte(EMAC_Section.EMACValueList[EMACCount].CameraType),
                    NextCameraIndex = Convert.ToByte(EMAC_Section.EMACValueList[EMACCount].NextCameraIndex),
                    EMAC_UnkBytes1 = Convert.ToByte(EMAC_Section.EMACValueList[EMACCount].EMAC_UnkBytes1),
                    EMAC_ITOP_CameraIndex = Convert.ToByte(EMAC_Section.EMACValueList[EMACCount].EMAC_ITOP_CameraIndex),
                    RouteSpeed = Convert.ToUInt16(EMAC_Section.EMACValueList[EMACCount].SpeedSettings.RouteSpeed),
                    FOVSpeed = Convert.ToUInt16(EMAC_Section.EMACValueList[EMACCount].SpeedSettings.FOVSpeed),
                    ViewpointSpeed = Convert.ToUInt16(EMAC_Section.EMACValueList[EMACCount].SpeedSettings.ViewpointSpeed),
                    EMAC_UnkBytes2 = Convert.ToByte(EMAC_Section.EMACValueList[EMACCount].EMAC_UnkBytes2),
                    EMAC_UnkBytes3 = Convert.ToByte(EMAC_Section.EMACValueList[EMACCount].EMAC_UnkBytes3),
                    EMAC_Position = new Vector3D(PX, PY, PZ),
                    EMAC_Rotation = new Vector3D(RX, RY, RZ),
                    FOVAngle_Start = Convert.ToSingle(EMAC_Section.EMACValueList[EMACCount].FOVAngleSettings.FOVAngle_Start),
                    FOVAngle_End = Convert.ToSingle(EMAC_Section.EMACValueList[EMACCount].FOVAngleSettings.FOVAngle_End),
                    Viewpoint_Start = new Vector3D(VP_Start_PX, VP_Start_PY, VP_Start_PZ),
                    Viewpoint_Destination = new Vector3D(VP_Destination_PX, VP_Destination_PY, VP_Destination_PZ),
                    Camera_Active_Time = Convert.ToSingle(EMAC_Section.EMACValueList[EMACCount].Camera_Active_Time)
                };

                EMAC_Value_List.Add(EMAC_Values);
            }

            EMAC.EMACValue_List = EMAC_Value_List;

            uint CameraPos = KMPs.KMPWriter.Write_EMAC(bw1, EMAC);
            #endregion

            #region TPGJ
            KMPs.KMPFormat.KMPSection.TPGJ_Section TPGJ = new KMPs.KMPFormat.KMPSection.TPGJ_Section
            {
                TPGJHeader = new char[] { 'T', 'P', 'G', 'J' },
                NumOfEntries = Convert.ToUInt16(TPGJ_Section.TPGJValueList.Count),
                AdditionalValue = 0,
                TPGJValue_List = null
            };

            List<KMPs.KMPFormat.KMPSection.TPGJ_Section.TPGJValue> TPGJ_Value_List = new List<KMPs.KMPFormat.KMPSection.TPGJ_Section.TPGJValue>();

            for (int TPGJCount = 0; TPGJCount < TPGJ_Section.TPGJValueList.Count; TPGJCount++)
            {
                #region Transform Value
                double PX = TPGJ_Section.TPGJValueList[TPGJCount].Positions.X;
                double PY = TPGJ_Section.TPGJValueList[TPGJCount].Positions.Y;
                double PZ = TPGJ_Section.TPGJValueList[TPGJCount].Positions.Z;

                double RX = HTK_3DES.TSRSystem.AngleToRadian(TPGJ_Section.TPGJValueList[TPGJCount].Rotations.X);
                double RY = HTK_3DES.TSRSystem.AngleToRadian(TPGJ_Section.TPGJValueList[TPGJCount].Rotations.Y);
                double RZ = HTK_3DES.TSRSystem.AngleToRadian(TPGJ_Section.TPGJValueList[TPGJCount].Rotations.Z);
                #endregion

                KMPs.KMPFormat.KMPSection.TPGJ_Section.TPGJValue TPGJ_Values = new KMPs.KMPFormat.KMPSection.TPGJ_Section.TPGJValue
                {
                    TPGJ_Position = new Vector3D(PX, PY, PZ),
                    TPGJ_Rotation = new Vector3D(RX, RY, RZ),
                    TPGJ_RespawnID = Convert.ToUInt16(TPGJ_Section.TPGJValueList[TPGJCount].TPGJ_RespawnID),
                    TPGJ_UnkBytes1 = Convert.ToUInt16(TPGJ_Section.TPGJValueList[TPGJCount].TPGJ_UnkBytes1),
                };

                TPGJ_Value_List.Add(TPGJ_Values);
            }

            TPGJ.TPGJValue_List = TPGJ_Value_List;

            uint JugemPointPos = KMPs.KMPWriter.Write_TPGJ(bw1, TPGJ);
            #endregion

            #region TPNC(Unused Section)
            KMPs.KMPFormat.KMPSection.TPNC_Section TPNC = new KMPs.KMPFormat.KMPSection.TPNC_Section
            {
                TPNCHeader = new char[] { 'T', 'P', 'N', 'C' },
                NumOfEntries = 0,
                AdditionalValue = 0
            };

            uint TPNCPos = KMPs.KMPWriter.Write_TPNC(bw1, TPNC);
            #endregion

            #region TPSM(Unused Section)
            KMPs.KMPFormat.KMPSection.TPSM_Section TPSM = new KMPs.KMPFormat.KMPSection.TPSM_Section
            {
                TPSMHeader = new char[] { 'T', 'P', 'S', 'M' },
                NumOfEntries = 0,
                AdditionalValue = 0
            };

            uint TPSMPos = KMPs.KMPWriter.Write_TPSM(bw1, TPSM);
            #endregion

            #region IGTS
            KMPs.KMPFormat.KMPSection.IGTS_Section IGTS = new KMPs.KMPFormat.KMPSection.IGTS_Section
            {
                IGTSHeader = new char[] { 'I', 'G', 'T', 'S' },
                Unknown1 = IGTS_Section.Unknown1,
                LapCount = IGTS_Section.LapCount,
                PolePosition = IGTS_Section.PolePosition,
                Unknown2 = IGTS_Section.Unknown2,
                Unknown3 = IGTS_Section.Unknown3,
                RGBAColor = new KMPs.KMPFormat.KMPSection.IGTS_Section.RGBA
                {
                    R = IGTS_Section.RGBAColor.R,
                    G = IGTS_Section.RGBAColor.G,
                    B = IGTS_Section.RGBAColor.B,
                    A = IGTS_Section.RGBAColor.A
                },
                FlareAlpha = IGTS_Section.FlareAlpha
            };

            uint StageInfoPos = KMPs.KMPWriter.Write_IGTS(bw1, IGTS);
            #endregion

            #region SROC(Unused Section)
            KMPs.KMPFormat.KMPSection.SROC_Section SROC = new KMPs.KMPFormat.KMPSection.SROC_Section
            {
                SROCHeader = new char[] { 'S', 'R', 'O', 'C' },
                NumOfEntries = 0,
                AdditionalValue = 0
            };

            uint SROCPos = KMPs.KMPWriter.Write_SROC(bw1, SROC);
            #endregion

            if (HPLG_TPLG_Section.HPLGValueList.Count != 0)
            {
                List<KMPs.KMPFormat.KMPSection.TPLG_Section.TPLGValue> TPLG_Values_List = new List<KMPs.KMPFormat.KMPSection.TPLG_Section.TPLGValue>();
                List<KMPs.KMPFormat.KMPSection.HPLG_Section.HPLGValue> HPLG_Values_List = new List<KMPs.KMPFormat.KMPSection.HPLG_Section.HPLGValue>();

                int StartPoint = 0;
                for (int HPLGCount = 0; HPLGCount < HPLG_TPLG_Section.HPLGValueList.Count; HPLGCount++)
                {
                    KMPs.KMPFormat.KMPSection.HPLG_Section.HPLGValue HPLG_Values = new KMPs.KMPFormat.KMPSection.HPLG_Section.HPLGValue
                    {
                        HPLG_StartPoint = Convert.ToByte(StartPoint),
                        HPLG_Length = Convert.ToByte(HPLG_TPLG_Section.HPLGValueList[HPLGCount].TPLGValueList.Count),
                        HPLG_PreviewGroup = new KMPs.KMPFormat.KMPSection.HPLG_Section.HPLGValue.HPLG_PreviewGroups
                        {
                            Prev0 = Convert.ToByte(HPLG_TPLG_Section.HPLGValueList[HPLGCount].HPLG_PreviewGroup.Prev0),
                            Prev1 = Convert.ToByte(HPLG_TPLG_Section.HPLGValueList[HPLGCount].HPLG_PreviewGroup.Prev1),
                            Prev2 = Convert.ToByte(HPLG_TPLG_Section.HPLGValueList[HPLGCount].HPLG_PreviewGroup.Prev2),
                            Prev3 = Convert.ToByte(HPLG_TPLG_Section.HPLGValueList[HPLGCount].HPLG_PreviewGroup.Prev3),
                            Prev4 = Convert.ToByte(HPLG_TPLG_Section.HPLGValueList[HPLGCount].HPLG_PreviewGroup.Prev4),
                            Prev5 = Convert.ToByte(HPLG_TPLG_Section.HPLGValueList[HPLGCount].HPLG_PreviewGroup.Prev5),
                        },
                        HPLG_NextGroup = new KMPs.KMPFormat.KMPSection.HPLG_Section.HPLGValue.HPLG_NextGroups
                        {
                            Next0 = Convert.ToByte(HPLG_TPLG_Section.HPLGValueList[HPLGCount].HPLG_NextGroup.Next0),
                            Next1 = Convert.ToByte(HPLG_TPLG_Section.HPLGValueList[HPLGCount].HPLG_NextGroup.Next1),
                            Next2 = Convert.ToByte(HPLG_TPLG_Section.HPLGValueList[HPLGCount].HPLG_NextGroup.Next2),
                            Next3 = Convert.ToByte(HPLG_TPLG_Section.HPLGValueList[HPLGCount].HPLG_NextGroup.Next3),
                            Next4 = Convert.ToByte(HPLG_TPLG_Section.HPLGValueList[HPLGCount].HPLG_NextGroup.Next4),
                            Next5 = Convert.ToByte(HPLG_TPLG_Section.HPLGValueList[HPLGCount].HPLG_NextGroup.Next5),
                        },
                        RouteSetting = Convert.ToUInt32(HPLG_TPLG_Section.HPLGValueList[HPLGCount].RouteSettings.RouteSettingValue),
                        HPLG_UnkBytes2 = Convert.ToUInt32(HPLG_TPLG_Section.HPLGValueList[HPLGCount].HPLG_UnkBytes2)
                    };

                    HPLG_Values_List.Add(HPLG_Values);

                    for(int TPLGCount = 0; TPLGCount < HPLG_TPLG_Section.HPLGValueList[HPLGCount].TPLGValueList.Count; TPLGCount++)
                    {
                        double PX = HPLG_TPLG_Section.HPLGValueList[HPLGCount].TPLGValueList[TPLGCount].Positions.X;
                        double PY = HPLG_TPLG_Section.HPLGValueList[HPLGCount].TPLGValueList[TPLGCount].Positions.Y;
                        double PZ = HPLG_TPLG_Section.HPLGValueList[HPLGCount].TPLGValueList[TPLGCount].Positions.Z;

                        KMPs.KMPFormat.KMPSection.TPLG_Section.TPLGValue TPLG_Values = new KMPs.KMPFormat.KMPSection.TPLG_Section.TPLGValue
                        {
                            TPLG_Position = new Vector3D(PX, PY, PZ),
                            TPLG_PointScaleValue = Convert.ToSingle(HPLG_TPLG_Section.HPLGValueList[HPLGCount].TPLGValueList[TPLGCount].TPLG_PointScaleValue),
                            TPLG_UnkBytes1 = HPLG_TPLG_Section.HPLGValueList[HPLGCount].TPLGValueList[TPLGCount].TPLG_UnkBytes1,
                            TPLG_UnkBytes2 = Convert.ToUInt16(HPLG_TPLG_Section.HPLGValueList[HPLGCount].TPLGValueList[TPLGCount].TPLG_UnkBytes2)
                        };

                        TPLG_Values_List.Add(TPLG_Values);

                        StartPoint++;
                    }
                }

                KMPs.KMPFormat.KMPSection.TPLG_Section TPLG = new KMPs.KMPFormat.KMPSection.TPLG_Section
                {
                    TPLGHeader = new char[] { 'T', 'P', 'L', 'G' },
                    NumOfEntries = Convert.ToUInt16(TPLG_Values_List.Count),
                    AdditionalValue = 0,
                    TPLGValue_List = TPLG_Values_List
                };

                KMPs.KMPFormat.KMPSection.HPLG_Section HPLG = new KMPs.KMPFormat.KMPSection.HPLG_Section
                {
                    HPLGHeader = new char[] { 'H', 'P', 'L', 'G' },
                    NumOfEntries = Convert.ToUInt16(HPLG_Values_List.Count),
                    AdditionalValue = 0,
                    HPLGValue_List = HPLG_Values_List
                };

                tPLG_HPLG_WritePosition = KMPs.KMPWriter.Write_TPLG_HPLG(bw1, TPLG, HPLG);
            }
            if (HPLG_TPLG_Section.HPLGValueList.Count == 0)
            {
                KMPs.KMPFormat.KMPSection.TPLG_Section TPLG = new KMPs.KMPFormat.KMPSection.TPLG_Section
                {
                    TPLGHeader = new char[] { 'T', 'P', 'L', 'G' },
                    NumOfEntries = 0,
                    AdditionalValue = 0,
                    TPLGValue_List = new List<KMPs.KMPFormat.KMPSection.TPLG_Section.TPLGValue>()
                };

                KMPs.KMPFormat.KMPSection.HPLG_Section HPLG = new KMPs.KMPFormat.KMPSection.HPLG_Section
                {
                    HPLGHeader = new char[] { 'H', 'P', 'L', 'G' },
                    NumOfEntries = 0,
                    AdditionalValue = 0,
                    HPLGValue_List = new List<KMPs.KMPFormat.KMPSection.HPLG_Section.HPLGValue>()
                };

                tPLG_HPLG_WritePosition = KMPs.KMPWriter.Write_TPLG_HPLG(bw1, TPLG, HPLG);
            }

            long KMPSize = bw1.BaseStream.Position;

            //位置を元に戻す
            bw1.BaseStream.Position = pos;

            KMPs.KMPFormat KMPFormat = new KMPs.KMPFormat
            {
                DMDCHeader = new char[] { 'D', 'M', 'D', 'C' },
                FileSize = Convert.ToUInt32(KMPSize),
                SectionCount = 18,
                DMDCHeaderSize = 88,
                VersionNumber = 3100,
                DMDC_SectionOffset = new KMPs.KMPFormat.DMDCSectionOffset
                {
                    TPTK_Offset = TPTKPos - 88,
                    TPNE_Offset = tPNE_HPNE_WritePosition.TPNE - 88,
                    HPNE_Offset = tPNE_HPNE_WritePosition.HPNE - 88,
                    TPTI_Offset = tPTI_HPTI_WritePosition.TPTI - 88,
                    HPTI_Offset = tPTI_HPTI_WritePosition.HPTI - 88,
                    TPKC_Offset = tPKC_HPKC_WritePosition.TPKC - 88,
                    HPKC_Offset = tPKC_HPKC_WritePosition.HPKC - 88,
                    JBOG_Offset = ObjPos - 88,
                    ITOP_Offset = RoutePos - 88,
                    AERA_Offset = AreaPos - 88,
                    EMAC_Offset = CameraPos - 88,
                    TPGJ_Offset = JugemPointPos - 88,
                    TPNC_Offset = TPNCPos - 88,
                    TPSM_Offset = TPSMPos - 88,
                    IGTS_Offset = StageInfoPos - 88,
                    SROC_Offset = SROCPos - 88,
                    TPLG_Offset = tPLG_HPLG_WritePosition.TPLG - 88,
                    HPLG_Offset = tPLG_HPLG_WritePosition.HPLG - 88
                }
            };

            bw1.Write(KMPFormat.DMDCHeader);
            bw1.Write(KMPFormat.FileSize);
            bw1.Write(KMPFormat.SectionCount);
            bw1.Write(KMPFormat.DMDCHeaderSize);
            bw1.Write(KMPFormat.VersionNumber);
            bw1.Write(KMPFormat.DMDC_SectionOffset.TPTK_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.TPNE_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.HPNE_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.TPTI_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.HPTI_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.TPKC_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.HPKC_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.JBOG_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.ITOP_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.AERA_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.EMAC_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.TPGJ_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.TPNC_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.TPSM_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.IGTS_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.SROC_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.TPLG_Offset);
            bw1.Write(KMPFormat.DMDC_SectionOffset.HPLG_Offset);

            bw1.Close();
            fs1.Close();
        }

        private void ViewportTypeChange_TSM_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem Viewport_Type_3D = (ToolStripMenuItem)sender;
            Viewport_Type_3D.Checked = !Viewport_Type_3D.Checked;

            if (Viewport_Type_3D.Checked == true)
            {
                //Projection mode
                render.MainViewPort.Orthographic = true;
                render.MainViewPort.CameraController.IsTouchZoomEnabled = false;

                //Disable the ability to rotate the camera by dragging the Viewport
                render.MainViewPort.CameraController.IsRotationEnabled = false;

            }
            if(Viewport_Type_3D.Checked == false)
            {
                render.MainViewPort.Orthographic = false;
                render.MainViewPort.CameraController.IsTouchZoomEnabled = true;
                render.MainViewPort.CameraController.IsRotationEnabled = true;
            }
        }

        private void AddKMPSection_Click(object sender, EventArgs e)
        {
            if(tabControl1.SelectedIndex == 0)
            {
                if (KMPSectionComboBox.Text == "EnemyRoutes")
                {
                    KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue hPNEValue = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue
                    {
                        GroupID = KMP_Group_ListBox.Items.Count,
                        HPNENextGroups = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.HPNE_NextGroups(),
                        HPNEPreviewGroups = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.HPNE_PreviewGroups(),
                        HPNE_UnkBytes1 = 0,
                        TPNEValueList = new List<KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue>()
                    };

                    HPNE_TPNE_Section.HPNEValueList.Add(hPNEValue);

                    KMP_Group_ListBox.Items.Add(hPNEValue);

                    //Rail
                    HTK_3DES.PathTools.Rail KMP_EnemyRoute_Rail = new HTK_3DES.PathTools.Rail
                    {
                        TV3D_List = new List<TubeVisual3D>(),
                        MV3D_List = new List<ModelVisual3D>()
                    };

                    //Add
                    KMPViewportObject.EnemyRoute_Rail_List.Add(KMP_EnemyRoute_Rail);
                }
                if (KMPSectionComboBox.Text == "ItemRoutes")
                {
                    KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue hPTIValue = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue
                    {
                        GroupID = KMP_Group_ListBox.Items.Count,
                        HPTI_NextGroup = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.HPTI_NextGroups(),
                        HPTI_PreviewGroup = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.HPTI_PreviewGroups(),
                        TPTIValueList = new List<KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue>()
                    };

                    HPTI_TPTI_Section.HPTIValueList.Add(hPTIValue);

                    KMP_Group_ListBox.Items.Add(hPTIValue);

                    //Rail
                    HTK_3DES.PathTools.Rail KMP_ItemRoute_Rail = new HTK_3DES.PathTools.Rail
                    {
                        TV3D_List = new List<TubeVisual3D>(),
                        MV3D_List = new List<ModelVisual3D>()
                    };

                    //Add
                    KMPViewportObject.ItemRoute_Rail_List.Add(KMP_ItemRoute_Rail);
                }
                if (KMPSectionComboBox.Text == "CheckPoint")
                {
                    KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue hPKCValue = new KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue
                    {
                        GroupID = KMP_Group_ListBox.Items.Count,
                        HPKC_NextGroup = new KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.HPKC_NextGroups(),
                        HPKC_PreviewGroup = new KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.HPKC_PreviewGroups(),
                        HPKC_UnkBytes1 = 0,
                        TPKCValueList = new List<KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue>()
                    };

                    HPKC_TPKC_Section.HPKCValueList.Add(hPKCValue);

                    KMP_Group_ListBox.Items.Add(hPKCValue);

                    //Checkpoint_Rails
                    HTK_3DES.KMP_3DCheckpointSystem.Checkpoint checkpoint = new HTK_3DES.KMP_3DCheckpointSystem.Checkpoint
                    {
                        Checkpoint_Left = new HTK_3DES.PathTools.Rail
                        {
                            LV3D_List = new List<LinesVisual3D>(),
                            TV3D_List = new List<TubeVisual3D>(),
                            MV3D_List = new List<ModelVisual3D>()
                        },
                        Checkpoint_Right = new HTK_3DES.PathTools.Rail
                        {
                            LV3D_List = new List<LinesVisual3D>(),
                            TV3D_List = new List<TubeVisual3D>(),
                            MV3D_List = new List<ModelVisual3D>()
                        },
                        Checkpoint_Line = new List<LinesVisual3D>(),
                        Checkpoint_Tube = new List<TubeVisual3D>()
                    };

                    //Add
                    KMPViewportObject.Checkpoint_Rail.Add(checkpoint);
                }
                if (KMPSectionComboBox.Text == "Route")
                {
                    KMPPropertyGridSettings.ITOP_Section.ITOP_Route iTOP_Route = new KMPPropertyGridSettings.ITOP_Section.ITOP_Route
                    {
                        GroupID = KMP_Group_ListBox.Items.Count,
                        ITOP_RouteSetting1 = 0,
                        ITOP_RouteSetting2 = 0,
                        ITOP_PointList = new List<KMPPropertyGridSettings.ITOP_Section.ITOP_Route.ITOP_Point>()
                    };

                    ITOP_Section.ITOP_RouteList.Add(iTOP_Route);

                    KMP_Group_ListBox.Items.Add(iTOP_Route);

                    //Rail
                    HTK_3DES.PathTools.Rail Route_Rail = new HTK_3DES.PathTools.Rail
                    {
                        TV3D_List = new List<TubeVisual3D>(),
                        MV3D_List = new List<ModelVisual3D>()
                    };

                    //Add
                    KMPViewportObject.Routes_List.Add(Route_Rail);
                }
                if (KMPSectionComboBox.Text == "GlideRoutes")
                {
                    KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue hPLGValue = new KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue
                    {
                        GroupID = KMP_Group_ListBox.Items.Count,
                        HPLG_NextGroup = new KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.HPLG_NextGroups(),
                        HPLG_PreviewGroup = new KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.HPLG_PreviewGroups(),
                        RouteSettings = new KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.RouteSetting
                        {
                            RouteSettingValue = 0,
                        },
                        HPLG_UnkBytes2 = 0,
                        TPLGValueList = new List<KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.TPLGValue>()
                    };

                    HPLG_TPLG_Section.HPLGValueList.Add(hPLGValue);

                    KMP_Group_ListBox.Items.Add(hPLGValue);

                    //Rail
                    HTK_3DES.PathTools.Rail GlideRoute_Rail = new HTK_3DES.PathTools.Rail
                    {
                        TV3D_List = new List<TubeVisual3D>(),
                        MV3D_List = new List<ModelVisual3D>()
                    };

                    //Add
                    KMPViewportObject.GlideRoute_Rail_List.Add(GlideRoute_Rail);
                }
            }
            if(tabControl1.SelectedIndex == 1)
            {
                //カメラの位置を取得
                Point3D Pos = render.ViewportPosition(UserControl1.PositionMode.CameraPos);

                if (KMPSectionComboBox.Text == "KartPoint")
                {
                    KMPPropertyGridSettings.TPTK_Section.TPTKValue tPTKValue = new KMPPropertyGridSettings.TPTK_Section.TPTKValue
                    {
                        ID = KMP_Path_ListBox.Items.Count,
                        Player_Index = 0,
                        Position_Value = new KMPPropertyGridSettings.TPTK_Section.TPTKValue.Position
                        {
                            X = (float)Pos.X,
                            Y = (float)Pos.Y,
                            Z = (float)Pos.Z
                        },
                        Rotate_Value = new KMPPropertyGridSettings.TPTK_Section.TPTKValue.Rotation
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        },
                        TPTK_UnkBytes = 0
                    };

                    TPTK_Section.TPTKValueList.Add(tPTKValue);

                    KMP_Path_ListBox.Items.Add(tPTKValue);

                    #region Add Model(StartPosition)
                    HTK_3DES.TSRSystem.Transform_Value StartPosition_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                    {
                        Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                        {
                            X = tPTKValue.Position_Value.X,
                            Y = tPTKValue.Position_Value.Y,
                            Z = tPTKValue.Position_Value.Z
                        },
                        Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                        {
                            X = 10,
                            Y = 10,
                            Z = 10
                        },
                        Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                        {
                            X = tPTKValue.Rotate_Value.X,
                            Y = tPTKValue.Rotate_Value.Y,
                            Z = tPTKValue.Rotate_Value.Z
                        }
                    };

                    ModelVisual3D dv3D_StartPositionOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\StartPosition\\StartPosition.obj");

                    //モデルの名前と番号を文字列に格納(情報化)
                    HTK_3DES.TSRSystem.SetString_MV3D(dv3D_StartPositionOBJ, "StartPosition " + tPTKValue.ID + " " + -1);

                    HTK_3DES.TransformMV3D.Transform_MV3D(StartPosition_transform_Value, dv3D_StartPositionOBJ);

                    KMPViewportObject.StartPosition_MV3DList.Add(dv3D_StartPositionOBJ);

                    render.MainViewPort.Children.Add(dv3D_StartPositionOBJ);

                    HTK_3DES.TSRSystem.GC_Dispose(dv3D_StartPositionOBJ);
                    #endregion
                }
                if (KMPSectionComboBox.Text == "EnemyRoutes")
                {
                    if (KMP_Group_ListBox.SelectedIndex != -1)
                    {
                        KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue tPNEValue = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue
                        {
                            Group_ID = KMP_Group_ListBox.SelectedIndex,
                            ID = KMP_Path_ListBox.Items.Count,
                            Positions = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.Position
                            {
                                X = (float)Pos.X,
                                Y = (float)Pos.Y,
                                Z = (float)Pos.Z
                            },
                            Control = 1,
                            MushSettings = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.MushSetting
                            {
                                MushSettingValue = 0
                            },
                            DriftSettings = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.DriftSetting
                            {
                                DriftSettingValue = 0
                            },
                            FlagSettings = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.FlagSetting
                            {
                                Flags = 0
                            },
                            PathFindOptions = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.PathFindOption
                            {
                                PathFindOptionValue = 0
                            },
                            MaxSearchYOffset = new KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue.MaxSearch_YOffset
                            {
                                MaxSearchYOffsetValue = 0
                            }
                        };

                        HPNE_TPNE_Section.HPNEValueList[KMP_Group_ListBox.SelectedIndex].TPNEValueList.Add(tPNEValue);

                        KMP_Path_ListBox.Items.Add(tPNEValue);

                        #region Add Model(EnemyRoutes)
                        HTK_3DES.TSRSystem.Transform_Value EnemyPoint_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                        {
                            Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                            {
                                X = tPNEValue.Positions.X,
                                Y = tPNEValue.Positions.Y,
                                Z = tPNEValue.Positions.Z
                            },
                            Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                            {
                                X = tPNEValue.Control * 100,
                                Y = tPNEValue.Control * 100,
                                Z = tPNEValue.Control * 100
                            },
                            Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                            {
                                X = 0,
                                Y = 0,
                                Z = 0
                            }
                        };

                        ModelVisual3D dv3D_EnemyPathOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\EnemyPath\\EnemyPath.obj");

                        //モデルの名前と番号を文字列に格納(情報化)
                        HTK_3DES.TSRSystem.SetString_MV3D(dv3D_EnemyPathOBJ, "EnemyRoute " + tPNEValue.ID + " " + tPNEValue.Group_ID);

                        HTK_3DES.TransformMV3D.Transform_MV3D(EnemyPoint_transform_Value, dv3D_EnemyPathOBJ);

                        //Add Rail => MV3DList
                        KMPViewportObject.EnemyRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].MV3D_List.Add(dv3D_EnemyPathOBJ);

                        render.MainViewPort.Children.Add(dv3D_EnemyPathOBJ);
                        #endregion

                        List<Point3D> point3Ds = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.EnemyRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].MV3D_List);
                        KMPViewportObject.EnemyRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].TV3D_List = HTK_3DES.PathTools.DrawPath_Tube(render, point3Ds, 10.0, Colors.Orange);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Group : The group may not be selected or it may be empty.");
                    }
                }
                if (KMPSectionComboBox.Text == "ItemRoutes")
                {
                    if (KMP_Group_ListBox.SelectedIndex != -1)
                    {
                        KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue tPTIValue = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue
                        {
                            Group_ID = KMP_Group_ListBox.SelectedIndex,
                            ID = KMP_Path_ListBox.Items.Count,
                            TPTI_Positions = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue.TPTI_Position
                            {
                                X = (float)Pos.X,
                                Y = (float)Pos.Y,
                                Z = (float)Pos.Z
                            },
                            TPTI_PointSize = 1,
                            GravityModeSettings = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue.GravityModeSetting
                            {
                                GravityModeValue = 0
                            },
                            PlayerScanRadiusSettings = new KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue.PlayerScanRadiusSetting
                            {
                                PlayerScanRadiusValue = 0
                            }
                        };

                        HPTI_TPTI_Section.HPTIValueList[KMP_Group_ListBox.SelectedIndex].TPTIValueList.Add(tPTIValue);

                        KMP_Path_ListBox.Items.Add(tPTIValue);

                        #region Add Model(ItemRoutes)
                        HTK_3DES.TSRSystem.Transform_Value ItemPoint_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                        {
                            Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                            {
                                X = tPTIValue.TPTI_Positions.X,
                                Y = tPTIValue.TPTI_Positions.Y,
                                Z = tPTIValue.TPTI_Positions.Z
                            },
                            Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                            {
                                X = tPTIValue.TPTI_PointSize * 100,
                                Y = tPTIValue.TPTI_PointSize * 100,
                                Z = tPTIValue.TPTI_PointSize * 100
                            },
                            Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                            {
                                X = 0,
                                Y = 0,
                                Z = 0
                            }
                        };

                        ModelVisual3D dv3D_ItemPathOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\ItemPath\\ItemPath.obj");

                        //モデルの名前と番号を文字列に格納(情報化)
                        HTK_3DES.TSRSystem.SetString_MV3D(dv3D_ItemPathOBJ, "ItemRoute " + tPTIValue.ID + " " + tPTIValue.Group_ID);

                        HTK_3DES.TransformMV3D.Transform_MV3D(ItemPoint_transform_Value, dv3D_ItemPathOBJ);

                        //Add Rail => MV3DList
                        KMPViewportObject.ItemRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].MV3D_List.Add(dv3D_ItemPathOBJ);

                        render.MainViewPort.Children.Add(dv3D_ItemPathOBJ);
                        #endregion

                        List<Point3D> point3Ds = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.ItemRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].MV3D_List);
                        KMPViewportObject.ItemRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].TV3D_List = HTK_3DES.PathTools.DrawPath_Tube(render, point3Ds, 10.0, Colors.Green);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Group : The group may not be selected or it may be empty.");
                    }
                }
                if (KMPSectionComboBox.Text == "CheckPoint")
                {
                    if (KMP_Group_ListBox.SelectedIndex != -1)
                    {
                        KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue tPKCValue = new KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue
                        {
                            Group_ID = KMP_Group_ListBox.SelectedIndex,
                            ID = KMP_Path_ListBox.Items.Count,
                            Position_2D_Left = new KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue.Position2D_Left
                            {
                                X = (float)Pos.X,
                                Y = (float)Pos.Z
                            },
                            Position_2D_Right = new KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue.Position2D_Right
                            {
                                X = (float)Pos.X,
                                Y = (float)Pos.Z
                            },
                            TPKC_RespawnID = 0xFF,
                            TPKC_Checkpoint_Type = 0,
                            TPKC_NextCheckPoint = 0xFF,
                            TPKC_PreviousCheckPoint = 0xFF,
                            TPKC_UnkBytes1 = 0,
                            TPKC_UnkBytes2 = 0,
                            TPKC_UnkBytes3 = 0,
                            TPKC_UnkBytes4 = 0
                        };

                        HPKC_TPKC_Section.HPKCValueList[KMP_Group_ListBox.SelectedIndex].TPKCValueList.Add(tPKCValue);

                        KMP_Path_ListBox.Items.Add(tPKCValue);

                        #region Create
                        var P2D_Left = tPKCValue.Position_2D_Left;
                        Vector2 P2DLeftToVector2 = new Vector2(Convert.ToSingle(P2D_Left.X), Convert.ToSingle(P2D_Left.Y));
                        Point3D P3DLeft = KMPs.KMPHelper.Vector3DTo2DConverter.Vector2DTo3D(P2DLeftToVector2, KMPs.KMPHelper.Vector3DTo2DConverter.Axis_Up.Y).ToPoint3D();
                        P3DLeft.Y = Convert.ToDouble(textBox1.Text);

                        #region Transform(Left)
                        HTK_3DES.TSRSystem.Transform_Value P2DLeft_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                        {
                            Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                            {
                                X = P3DLeft.X,
                                Y = P3DLeft.Y,
                                Z = P3DLeft.Z
                            },
                            Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                            {
                                X = 50,
                                Y = 50,
                                Z = 50
                            },
                            Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                            {
                                X = 0,
                                Y = 0,
                                Z = 0
                            }
                        };

                        ModelVisual3D dv3D_CheckpointLeftOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Checkpoint\\LeftPoint\\Checkpoint_Left.obj");

                        //モデルの名前と番号を文字列に格納(情報化)
                        HTK_3DES.TSRSystem.SetString_MV3D(dv3D_CheckpointLeftOBJ, "Checkpoint_Left " + tPKCValue.ID + " " + tPKCValue.Group_ID);

                        HTK_3DES.TransformMV3D.Transform_MV3D(P2DLeft_transform_Value, dv3D_CheckpointLeftOBJ);

                        KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Left.MV3D_List.Add(dv3D_CheckpointLeftOBJ);

                        render.MainViewPort.Children.Add(dv3D_CheckpointLeftOBJ);

                        HTK_3DES.TSRSystem.GC_Dispose(dv3D_CheckpointLeftOBJ);
                        #endregion

                        var P2D_Right = tPKCValue.Position_2D_Right;
                        Vector2 P2DRightToVector2 = new Vector2(Convert.ToSingle(P2D_Right.X), Convert.ToSingle(P2D_Right.Y));
                        Point3D P3DRight = KMPs.KMPHelper.Vector3DTo2DConverter.Vector2DTo3D(P2DRightToVector2, KMPs.KMPHelper.Vector3DTo2DConverter.Axis_Up.Y).ToPoint3D();
                        P3DRight.Y = Convert.ToDouble(textBox1.Text);

                        #region Transform(Right)
                        HTK_3DES.TSRSystem.Transform_Value P2DRight_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                        {
                            Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                            {
                                X = P3DRight.X,
                                Y = P3DRight.Y,
                                Z = P3DRight.Z
                            },
                            Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                            {
                                X = 50,
                                Y = 50,
                                Z = 50
                            },
                            Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                            {
                                X = 0,
                                Y = 0,
                                Z = 0
                            }
                        };

                        ModelVisual3D dv3D_CheckpointRightOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Checkpoint\\RightPoint\\Checkpoint_Right.obj");

                        //モデルの名前と番号を文字列に格納(情報化)
                        HTK_3DES.TSRSystem.SetString_MV3D(dv3D_CheckpointRightOBJ, "Checkpoint_Right " + tPKCValue.ID + " " + tPKCValue.Group_ID);

                        HTK_3DES.TransformMV3D.Transform_MV3D(P2DRight_transform_Value, dv3D_CheckpointRightOBJ);

                        KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Right.MV3D_List.Add(dv3D_CheckpointRightOBJ);

                        render.MainViewPort.Children.Add(dv3D_CheckpointRightOBJ);

                        HTK_3DES.TSRSystem.GC_Dispose(dv3D_CheckpointRightOBJ);
                        #endregion

                        List<Point3D> point3Ds = new List<Point3D>();
                        point3Ds.Add(P3DLeft);
                        point3Ds.Add(P3DRight);

                        LinesVisual3D linesVisual3D = new LinesVisual3D
                        {
                            Points = new Point3DCollection(point3Ds),
                            Thickness = 1,
                            Color = Colors.Black
                        };

                        KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Line.Add(linesVisual3D);
                        render.MainViewPort.Children.Add(linesVisual3D);
                        #endregion

                        List<Point3D> point3Ds_Left = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Left.MV3D_List);
                        KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Left.LV3D_List = HTK_3DES.PathTools.DrawPath_Line(render, point3Ds_Left, 5, Colors.Green);

                        List<Point3D> point3Ds_Right = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Right.MV3D_List);
                        KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Right.LV3D_List = HTK_3DES.PathTools.DrawPath_Line(render, point3Ds_Right, 5, Colors.Red);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Group : The group may not be selected or it may be empty.");
                    }
                }
                if (KMPSectionComboBox.Text == "Obj")
                {
                    AddKMPObjectForm addKMPObjectForm = new AddKMPObjectForm();
                    addKMPObjectForm.ShowDialog();

                    var data = addKMPObjectForm.SelectedKMPObject_Info;

                    KMPPropertyGridSettings.JBOG_section.JBOGValue jBOGValue = new KMPPropertyGridSettings.JBOG_section.JBOGValue
                    {
                        ID = KMP_Path_ListBox.Items.Count,
                        ObjectName = data.Name,
                        ObjectID = data.ObjID,
                        JBOG_ITOP_RouteIDIndex = 65535,
                        JBOG_PresenceSetting = 7,
                        JBOG_UnkByte1 = "0000",
                        JBOG_UnkByte2 = "FFFF",
                        JBOG_UnkByte3 = 0,
                        Positions = new KMPPropertyGridSettings.JBOG_section.JBOGValue.Position
                        {
                            X = (float)Pos.X,
                            Y = (float)Pos.Y,
                            Z = (float)Pos.Z
                        },
                        Scales = new KMPPropertyGridSettings.JBOG_section.JBOGValue.Scale
                        {
                            X = 1,
                            Y = 1,
                            Z = 1
                        },
                        Rotations = new KMPPropertyGridSettings.JBOG_section.JBOGValue.Rotation
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        },
                        JOBJ_Specific_Setting = new KMPPropertyGridSettings.JBOG_section.JBOGValue.JBOG_SpecificSetting
                        {
                            Value0 = 0,
                            Value1 = 0,
                            Value2 = 0,
                            Value3 = 0,
                            Value4 = 0,
                            Value5 = 0,
                            Value6 = 0,
                            Value7 = 0
                        }
                    };

                    JBOG_Section.JBOGValueList.Add(jBOGValue);

                    KMP_Path_ListBox.Items.Add(jBOGValue);

                    #region Add Model(OBJ)
                    HTK_3DES.TSRSystem.Transform_Value OBJ_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                    {
                        Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                        {
                            X = jBOGValue.Positions.X,
                            Y = jBOGValue.Positions.Y,
                            Z = jBOGValue.Positions.Z
                        },
                        Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                        {
                            X = jBOGValue.Scales.X * 2,
                            Y = jBOGValue.Scales.Y * 2,
                            Z = jBOGValue.Scales.Z * 2
                        },
                        Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                        {
                            X = jBOGValue.Rotations.X,
                            Y = jBOGValue.Rotations.Y,
                            Z = jBOGValue.Rotations.Z
                        }
                    };

                    KMPs.KMPHelper.ObjFlowReader.ObjFlowXmlToObject objFlowXmlToObject = KMPs.KMPHelper.ObjFlowReader.ReadObjFlowXml("ObjFlowData.xml");
                    string Path = objFlowXmlToObject.ObjFlows.Find(x => x.ObjectID == data.ObjID).Path;
                    ModelVisual3D dv3D_OBJ = HTK_3DES.TSRSystem.OBJReader(Path);

                    //モデルの名前と番号を文字列に格納(情報化)
                    HTK_3DES.TSRSystem.SetString_MV3D(dv3D_OBJ, "OBJ " + jBOGValue.ID + " " + -1);

                    HTK_3DES.TransformMV3D.Transform_MV3D(OBJ_transform_Value, dv3D_OBJ);

                    KMPViewportObject.OBJ_MV3DList.Add(dv3D_OBJ);

                    render.MainViewPort.Children.Add(dv3D_OBJ);
                    #endregion
                }
                if (KMPSectionComboBox.Text == "Route")
                {
                    if (KMP_Group_ListBox.SelectedIndex != -1)
                    {
                        KMPPropertyGridSettings.ITOP_Section.ITOP_Route.ITOP_Point iTOP_Point = new KMPPropertyGridSettings.ITOP_Section.ITOP_Route.ITOP_Point
                        {
                            GroupID = KMP_Group_ListBox.SelectedIndex,
                            ID = KMP_Path_ListBox.Items.Count,
                            Positions = new KMPPropertyGridSettings.ITOP_Section.ITOP_Route.ITOP_Point.Position
                            {
                                X = (float)Pos.X,
                                Y = (float)Pos.Y,
                                Z = (float)Pos.Z
                            },
                            ITOP_PointSetting2 = 0,
                            ITOP_Point_RouteSpeed = 0
                        };

                        ITOP_Section.ITOP_RouteList[KMP_Group_ListBox.SelectedIndex].ITOP_PointList.Add(iTOP_Point);

                        KMP_Path_ListBox.Items.Add(iTOP_Point);

                        #region Add Model(Routes)
                        HTK_3DES.TSRSystem.Transform_Value JugemPath_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                        {
                            Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                            {
                                X = iTOP_Point.Positions.X,
                                Y = iTOP_Point.Positions.Y,
                                Z = iTOP_Point.Positions.Z
                            },
                            Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                            {
                                X = 10,
                                Y = 10,
                                Z = 10
                            },
                            Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                            {
                                X = 0,
                                Y = 0,
                                Z = 0
                            }
                        };

                        ModelVisual3D dv3D_RouteOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Routes\\Routes.obj");

                        //モデルの名前と番号を文字列に格納(情報化)
                        HTK_3DES.TSRSystem.SetString_MV3D(dv3D_RouteOBJ, "Routes " + iTOP_Point.ID + " " + iTOP_Point.GroupID);

                        HTK_3DES.TransformMV3D.Transform_MV3D(JugemPath_transform_Value, dv3D_RouteOBJ);

                        //AddMDL
                        KMPViewportObject.Routes_List[KMP_Group_ListBox.SelectedIndex].MV3D_List.Add(dv3D_RouteOBJ);

                        render.MainViewPort.Children.Add(dv3D_RouteOBJ);
                        #endregion

                        List<Point3D> point3Ds = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.Routes_List[KMP_Group_ListBox.SelectedIndex].MV3D_List);
                        KMPViewportObject.Routes_List[KMP_Group_ListBox.SelectedIndex].TV3D_List = HTK_3DES.PathTools.DrawPath_Tube(render, point3Ds, 10.0, Colors.Blue);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Group : The group may not be selected or it may be empty.");
                    }
                }
                if (KMPSectionComboBox.Text == "Area")
                {
                    KMPPropertyGridSettings.AERA_Section.AERAValue aERAValue = new KMPPropertyGridSettings.AERA_Section.AERAValue
                    {
                        ID = KMP_Path_ListBox.Items.Count,
                        Scales = new KMPPropertyGridSettings.AERA_Section.AERAValue.Scale
                        {
                            X = 1,
                            Y = 1,
                            Z = 1
                        },
                        Rotations = new KMPPropertyGridSettings.AERA_Section.AERAValue.Rotation
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        },
                        Positions = new KMPPropertyGridSettings.AERA_Section.AERAValue.Position
                        {
                            X = (float)Pos.X,
                            Y = (float)Pos.Y,
                            Z = (float)Pos.Z
                        },
                        AreaModeSettings = new KMPPropertyGridSettings.AERA_Section.AERAValue.AreaModeSetting
                        {
                            AreaModeValue = 0
                        },
                        //AreaMode = 0,
                        AreaType = 0,
                        AERA_EMACIndex = 0,
                        Priority = 0,
                        AERA_UnkByte1 = 0,
                        AERA_UnkByte2 = 0,
                        RouteID = 0,
                        EnemyID = 0,
                        AERA_UnkByte4 = 0
                    };

                    AERA_Section.AERAValueList.Add(aERAValue);

                    KMP_Path_ListBox.Items.Add(aERAValue);

                    #region Add Model(Area)
                    HTK_3DES.TSRSystem.Transform_Value Area_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                    {
                        Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                        {
                            X = aERAValue.Positions.X,
                            Y = aERAValue.Positions.Y,
                            Z = aERAValue.Positions.Z
                        },
                        Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                        {
                            X = aERAValue.Scales.X * 1000,
                            Y = aERAValue.Scales.Y * 1000,
                            Z = aERAValue.Scales.Z * 1000
                        },
                        Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                        {
                            X = aERAValue.Rotations.X,
                            Y = aERAValue.Rotations.Y,
                            Z = aERAValue.Rotations.Z
                        }
                    };

                    ModelVisual3D dv3D_AreaOBJ = null;
                    if (aERAValue.AreaModeSettings.AreaModeValue == 0) dv3D_AreaOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Area\\Area_Box\\Area_Box.obj");

                    //モデルの名前と番号を文字列に格納(情報化)
                    HTK_3DES.TSRSystem.SetString_MV3D(dv3D_AreaOBJ, "Area " + aERAValue.ID + " " + -1);

                    HTK_3DES.TransformMV3D.Transform_MV3D(Area_transform_Value, dv3D_AreaOBJ);

                    //Area_MV3D_List.Add(dv3D_AreaOBJ);
                    KMPViewportObject.Area_MV3DList.Add(dv3D_AreaOBJ);

                    render.MainViewPort.Children.Add(dv3D_AreaOBJ);
                    #endregion
                }
                if (KMPSectionComboBox.Text == "Camera")
                {
                    KMPPropertyGridSettings.EMAC_Section.EMACValue eMACValue = new KMPPropertyGridSettings.EMAC_Section.EMACValue
                    {
                        ID = KMP_Path_ListBox.Items.Count,
                        CameraType = 0,
                        NextCameraIndex = 0,
                        EMAC_UnkBytes1 = 0,
                        EMAC_ITOP_CameraIndex = 0,
                        SpeedSettings = new KMPPropertyGridSettings.EMAC_Section.EMACValue.SpeedSetting
                        {
                            RouteSpeed = 0,
                            FOVSpeed = 0,
                            ViewpointSpeed = 0
                        },
                        EMAC_UnkBytes2 = 0,
                        EMAC_UnkBytes3 = 0,
                        Positions = new KMPPropertyGridSettings.EMAC_Section.EMACValue.Position
                        {
                            X = (float)Pos.X,
                            Y = (float)Pos.Y,
                            Z = (float)Pos.Z
                        },
                        Rotations = new KMPPropertyGridSettings.EMAC_Section.EMACValue.Rotation
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        },
                        FOVAngleSettings = new KMPPropertyGridSettings.EMAC_Section.EMACValue.FOVAngleSetting
                        {
                            FOVAngle_Start = 0,
                            FOVAngle_End = 0
                        },
                        Viewpoint_Destination = new KMPPropertyGridSettings.EMAC_Section.EMACValue.ViewpointDestination
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        },
                        Viewpoint_Start = new KMPPropertyGridSettings.EMAC_Section.EMACValue.ViewpointStart
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        },
                        Camera_Active_Time = 0
                    };

                    EMAC_Section.EMACValueList.Add(eMACValue);

                    KMP_Path_ListBox.Items.Add(eMACValue);

                    #region Add Model(Camera)
                    HTK_3DES.TSRSystem.Transform_Value Camera_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                    {
                        Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                        {
                            X = eMACValue.Positions.X,
                            Y = eMACValue.Positions.Y,
                            Z = eMACValue.Positions.Z
                        },
                        Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                        {
                            X = 10,
                            Y = 10,
                            Z = 10
                        },
                        Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                        {
                            X = eMACValue.Rotations.X,
                            Y = eMACValue.Rotations.Y,
                            Z = eMACValue.Rotations.Z
                        }
                    };

                    ModelVisual3D dv3D_CameraOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Camera\\Camera.obj");

                    //モデルの名前と番号を文字列に格納(情報化)
                    HTK_3DES.TSRSystem.SetString_MV3D(dv3D_CameraOBJ, "Camera " + eMACValue.ID + " " + -1);

                    HTK_3DES.TransformMV3D.Transform_MV3D(Camera_transform_Value, dv3D_CameraOBJ);

                    //Camera_MV3D_List.Add(dv3D_CameraOBJ);
                    KMPViewportObject.Camera_MV3DList.Add(dv3D_CameraOBJ);

                    render.MainViewPort.Children.Add(dv3D_CameraOBJ);
                    #endregion
                }
                if (KMPSectionComboBox.Text == "JugemPoint")
                {
                    KMPPropertyGridSettings.TPGJ_Section.TPGJValue tPGJValue = new KMPPropertyGridSettings.TPGJ_Section.TPGJValue
                    {
                        ID = KMP_Path_ListBox.Items.Count,
                        TPGJ_RespawnID = 65535,
                        Positions = new KMPPropertyGridSettings.TPGJ_Section.TPGJValue.Position
                        {
                            X = (float)Pos.X,
                            Y = (float)Pos.Y,
                            Z = (float)Pos.Z
                        },
                        Rotations = new KMPPropertyGridSettings.TPGJ_Section.TPGJValue.Rotation
                        {
                            X = 0,
                            Y = 0,
                            Z = 0
                        },
                        TPGJ_UnkBytes1 = 0
                    };

                    TPGJ_Section.TPGJValueList.Add(tPGJValue);

                    KMP_Path_ListBox.Items.Add(tPGJValue);

                    #region Add Model(RespawnPoint)
                    HTK_3DES.TSRSystem.Transform_Value RespawnPoint_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                    {
                        Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                        {
                            X = tPGJValue.Positions.X,
                            Y = tPGJValue.Positions.Y,
                            Z = tPGJValue.Positions.Z
                        },
                        Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                        {
                            X = 10,
                            Y = 10,
                            Z = 10
                        },
                        Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                        {
                            X = tPGJValue.Rotations.X,
                            Y = tPGJValue.Rotations.Y,
                            Z = tPGJValue.Rotations.Z
                        }
                    };

                    ModelVisual3D dv3D_RespawnPointOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\RespawnPoint\\RespawnPoint.obj");

                    //モデルの名前と番号を文字列に格納(情報化)
                    HTK_3DES.TSRSystem.SetString_MV3D(dv3D_RespawnPointOBJ, "RespawnPoint " + tPGJValue.ID + " " + -1);

                    HTK_3DES.TransformMV3D.Transform_MV3D(RespawnPoint_transform_Value, dv3D_RespawnPointOBJ);

                    //RespawnPoint_MV3D_List.Add(dv3D_RespawnPointOBJ);
                    KMPViewportObject.RespawnPoint_MV3DList.Add(dv3D_RespawnPointOBJ);

                    render.MainViewPort.Children.Add(dv3D_RespawnPointOBJ);
                    #endregion
                }
                if (KMPSectionComboBox.Text == "GlideRoutes")
                {
                    if (KMP_Group_ListBox.SelectedIndex != -1)
                    {
                        KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.TPLGValue tPLGValue = new KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.TPLGValue
                        {
                            GroupID = KMP_Group_ListBox.SelectedIndex,
                            ID = KMP_Path_ListBox.Items.Count,
                            Positions = new KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.TPLGValue.Position
                            {
                                X = (float)Pos.X,
                                Y = (float)Pos.Y,
                                Z = (float)Pos.Z
                            },
                            TPLG_PointScaleValue = 1,
                            TPLG_UnkBytes1 = 0,
                            TPLG_UnkBytes2 = 0
                        };

                        HPLG_TPLG_Section.HPLGValueList[KMP_Group_ListBox.SelectedIndex].TPLGValueList.Add(tPLGValue);

                        KMP_Path_ListBox.Items.Add(tPLGValue);

                        #region Add Model(GlideRoutes)
                        HTK_3DES.TSRSystem.Transform_Value GliderPoint_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                        {
                            Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                            {
                                X = tPLGValue.Positions.X,
                                Y = tPLGValue.Positions.Y,
                                Z = tPLGValue.Positions.Z
                            },
                            Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                            {
                                X = tPLGValue.TPLG_PointScaleValue * 10,
                                Y = tPLGValue.TPLG_PointScaleValue * 10,
                                Z = tPLGValue.TPLG_PointScaleValue * 10
                            },
                            Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                            {
                                X = 0,
                                Y = 0,
                                Z = 0
                            }
                        };

                        ModelVisual3D dv3D_GliderPathOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\GliderPath\\GliderPath.obj");

                        //モデルの名前と番号を文字列に格納(情報化)
                        HTK_3DES.TSRSystem.SetString_MV3D(dv3D_GliderPathOBJ, "GlideRoutes " + tPLGValue.ID + " " + tPLGValue.GroupID);

                        HTK_3DES.TransformMV3D.Transform_MV3D(GliderPoint_transform_Value, dv3D_GliderPathOBJ);

                        //Add model
                        KMPViewportObject.GlideRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].MV3D_List.Add(dv3D_GliderPathOBJ);

                        render.MainViewPort.Children.Add(dv3D_GliderPathOBJ);
                        #endregion

                        List<Point3D> point3Ds = HTK_3DES.PathTools.MV3DListToPoint3DList(KMPViewportObject.GlideRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].MV3D_List);
                        KMPViewportObject.GlideRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].TV3D_List = HTK_3DES.PathTools.DrawPath_Tube(render, point3Ds, 10.0, Colors.LightSkyBlue);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Group : The group may not be selected or it may be empty.");
                    }
                }
            }
        }

        #region [CustomBYAMLCollectionEditor] Delete
        /// <summary>
        /// Delete item by specifying index
        /// </summary>
        /// <typeparam name="T">T = BYAMLPropertyGridSettings.XXXX</typeparam>
        /// <param name="InputList">List<T></param>
        /// <param name="MV3DList">List<ModelVisual3D></param>
        /// <param name="Input_ListBox">Specifying a ListBox</param>
        /// <param name="Index">Index</param>
        public void DeleteItem<T>(List<T> InputList, List<ModelVisual3D> MV3DList, System.Windows.Forms.ListBox Input_ListBox, int Index)
        {
            InputList.RemoveAt(Index);
            render.MainViewPort.Children.Remove(MV3DList[Index]);
            MV3DList.RemoveAt(Index);
            Input_ListBox.Items.RemoveAt(Index);
        }

        /// <summary>
        /// Delete item by specifying index
        /// </summary>
        /// <typeparam name="T">T = BYAMLPropertyGridSettings.XXXX</typeparam>
        /// <param name="InputList">List<T></param>
        /// <param name="MV3DList">List<List<ModelVisual3D>></param>
        /// <param name="Input_ListBox">Specifying a ListBox</param>
        /// <param name="Index">Index</param>
        public void DeleteItem<T>(List<T> InputList, List<List<ModelVisual3D>> MV3DList, System.Windows.Forms.ListBox Input_ListBox, int Index)
        {
            InputList.RemoveAt(Index);

            for (int Del = 0; Del < MV3DList[Index].Count; Del++)
            {
                render.MainViewPort.Children.Remove(MV3DList[Index][Del]);
                render.UpdateLayout();
            }

            MV3DList.RemoveAt(Index);
            Input_ListBox.Items.RemoveAt(Index);
        }

        /// <summary>
        /// Delete item by specifying index
        /// </summary>
        /// <typeparam name="T">T = BYAMLPropertyGridSettings.XXXX</typeparam>
        /// <param name="InputList">List<T></param>
        /// <param name="MV3DList_1">List<List<ModelVisual3D>> 1</param>
        /// <param name="MV3DList_2">List<List<ModelVisual3D>> 2</param>
        /// <param name="Input_ListBox">Specifying a ListBox</param>
        /// <param name="Index">Index</param>
        public void DeleteItem<T>(List<T> InputList, List<List<ModelVisual3D>> MV3DList_1, List<List<ModelVisual3D>> MV3DList_2, System.Windows.Forms.ListBox Input_ListBox, int Index)
        {
            InputList.RemoveAt(Index);

            for (int Del = 0; Del < MV3DList_1[Index].Count; Del++)
            {
                render.MainViewPort.Children.Remove(MV3DList_1[Index][Del]);
                render.UpdateLayout();
            }

            MV3DList_1.RemoveAt(Index);

            for (int Del = 0; Del < MV3DList_2[Index].Count; Del++)
            {
                render.MainViewPort.Children.Remove(MV3DList_2[Index][Del]);
                render.UpdateLayout();
            }

            MV3DList_2.RemoveAt(Index);
            Input_ListBox.Items.RemoveAt(Index);
        }

        /// <summary>
        /// Get property
        /// </summary>
        /// <param name="obj">object</param>
        /// <param name="PropertyName">PropertyName</param>
        /// <returns>object</returns>
        public object GetBYAMLXmlProperty(object obj, string PropertyName)
        {
            Type type = obj.GetType();
            PropertyInfo propertyInfo = type.GetProperty(PropertyName);
            object GetValue = propertyInfo.GetValue(obj);
            return GetValue;
        }

        /// <summary>
        /// Set property
        /// </summary>
        /// <param name="obj">object</param>
        /// <param name="InputValue">Value to be entered</param>
        /// <param name="PropertyName">PropertyName</param>
        public void SetProperty(object obj, object InputValue, string PropertyName)
        {
            Type type = obj.GetType();
            PropertyInfo propertyInfo = type.GetProperty(PropertyName);
            propertyInfo.SetValue(obj, InputValue);
        }

        /// <summary>
        /// Modify the ID of the specified List<T>.
        /// </summary>
        /// <typeparam name="T">T = BYAMLPropertyGridSettings.XXXX</typeparam>
        /// <param name="Input">List<T></param>
        /// <paramref name="PropertyName"/>PropertyName</param>
        public void ReInputID<T>(List<T> Input, string PropertyName)
        {
            //再度IDを入れる
            for (int ReCountNum = 0; ReCountNum < Input.Count; ReCountNum++)
            {
                SetProperty(Input[ReCountNum], ReCountNum, PropertyName);
            }
        }

        /// <summary>
        /// Modify the ID of the specified List<HTK_3DES.PathTools.Rail>.
        /// </summary>
        /// <param name="InputRail">List<HTK_3DES.PathTools.Rail></param>
        /// <param name="InputName">ModelName</param>
        public void ReInputModelID(List<HTK_3DES.PathTools.Rail> InputRail, string InputName)
        {
            //再度IDを入れる
            for (int ReCountGrpNum = 0; ReCountGrpNum < InputRail.Count; ReCountGrpNum++)
            {
                HTK_3DES.PathTools.Rail rail = InputRail[ReCountGrpNum];

                for (int ReCountPtNum = 0; ReCountPtNum < rail.MV3D_List.Count; ReCountPtNum++)
                {
                    rail.MV3D_List[ReCountPtNum].SetName(InputName + " " + ReCountPtNum + " " + ReCountGrpNum);
                }
            }
        }

        /// <summary>
        /// Modify the ID of the specified List<HTK_3DES.PathTools.Rail>.
        /// </summary>
        /// <param name="InputRail">List<HTK_3DES.PathTools.Rail></param>
        /// <param name="InputName">ModelName</param>
        public void ReInputModelID(List<HTK_3DES.KMP_3DCheckpointSystem.Checkpoint> InputRailChk, string InputName_Left, string InputName_Right)
        {
            //再度IDを入れる
            for (int ReCountGrpNum = 0; ReCountGrpNum < InputRailChk.Count; ReCountGrpNum++)
            {
                HTK_3DES.KMP_3DCheckpointSystem.Checkpoint checkpoint = InputRailChk[ReCountGrpNum];

                for (int ChkLeftCount = 0; ChkLeftCount < checkpoint.Checkpoint_Left.MV3D_List.Count; ChkLeftCount++)
                {
                    checkpoint.Checkpoint_Left.MV3D_List[ChkLeftCount].SetName(InputName_Left + " " + ChkLeftCount + " " + ReCountGrpNum);
                }

                for (int ChkRightCount = 0; ChkRightCount < checkpoint.Checkpoint_Right.MV3D_List.Count; ChkRightCount++)
                {
                    checkpoint.Checkpoint_Right.MV3D_List[ChkRightCount].SetName(InputName_Right + " " + ChkRightCount + " " + ReCountGrpNum);
                }
            }
        }

        /// <summary>
        /// Modify the ID of the specified List<ModelVisual3D>.
        /// </summary>
        /// <param name="Input">List<ModelVisual3D></param>
        /// <param name="InputName">ModelName</param>
        public void ReInputModelID(List<ModelVisual3D> Input, string InputName)
        {
            //再度IDを入れる
            for (int ReCountNum = 0; ReCountNum < Input.Count; ReCountNum++)
            {
                Input[ReCountNum].SetName(InputName + " " + ReCountNum + " " + -1);
            }
        }

        /// <summary>
        /// Redraw the ListBox
        /// </summary>
        /// <typeparam name="T">T = BYAMLPropertyGridSettings.XXXX</typeparam>
        /// <param name="InputList">List<T></param>
        /// <param name="Input_ListBox">Specifying the ListBox to be redrawn</param>
        public void UpdateListBox<T>(List<T> InputList, System.Windows.Forms.ListBox Input_ListBox)
        {
            //ListBoxの再描画
            Input_ListBox.Items.Clear();
            Input_ListBox.Items.AddRange(InputList.Cast<object>().ToArray());
        }
        #endregion

        private void DeleteKMPSection_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                if (KMPSectionComboBox.Text == "EnemyRoutes")
                {
                    int N = KMP_Group_ListBox.SelectedIndex;
                    if (N != -1)
                    {
                        HTK_3DES.PathTools.Rail rail = KMPViewportObject.EnemyRoute_Rail_List[N];
                        HTK_3DES.PathTools.DeleteRail(render, rail);
                        KMPViewportObject.EnemyRoute_Rail_List.RemoveAt(N);

                        ReInputModelID(KMPViewportObject.EnemyRoute_Rail_List, "EnemyRoute");

                        HPNE_TPNE_Section.HPNEValueList.RemoveAt(N);
                        KMP_Group_ListBox.Items.RemoveAt(N);

                        ReInputID(HPNE_TPNE_Section.HPNEValueList, "GroupID");

                        for (int ReInputCount = 0; ReInputCount < HPNE_TPNE_Section.HPNEValueList.Count; ReInputCount++)
                        {
                            for(int i = 0; i < HPNE_TPNE_Section.HPNEValueList[ReInputCount].TPNEValueList.Count; i++)
                            {
                                SetProperty(HPNE_TPNE_Section.HPNEValueList[ReInputCount].TPNEValueList[i], ReInputCount, "Group_ID");
                            }
                        }

                        propertyGrid_KMP_Group.SelectedObject = null;
                        UpdateListBox(HPNE_TPNE_Section.HPNEValueList, KMP_Group_ListBox);

                        KMP_Path_ListBox.Items.Clear();
                        propertyGrid_KMP_Path.SelectedObject = null;

                        KMP_Group_ListBox.SelectedIndex = KMP_Group_ListBox.Items.Count - 1;
                    }
                }
                if (KMPSectionComboBox.Text == "ItemRoutes")
                {
                    int N = KMP_Group_ListBox.SelectedIndex;
                    if (N != -1)
                    {
                        HTK_3DES.PathTools.Rail rail = KMPViewportObject.ItemRoute_Rail_List[N];
                        HTK_3DES.PathTools.DeleteRail(render, rail);
                        KMPViewportObject.ItemRoute_Rail_List.RemoveAt(N);

                        ReInputModelID(KMPViewportObject.ItemRoute_Rail_List, "ItemRoute");

                        HPTI_TPTI_Section.HPTIValueList.RemoveAt(N);
                        KMP_Group_ListBox.Items.RemoveAt(N);

                        ReInputID(HPTI_TPTI_Section.HPTIValueList, "GroupID");

                        for (int ReInputCount = 0; ReInputCount < HPTI_TPTI_Section.HPTIValueList.Count; ReInputCount++)
                        {
                            for (int i = 0; i < HPTI_TPTI_Section.HPTIValueList[ReInputCount].TPTIValueList.Count; i++)
                            {
                                SetProperty(HPTI_TPTI_Section.HPTIValueList[ReInputCount].TPTIValueList[i], ReInputCount, "Group_ID");
                            }
                        }

                        propertyGrid_KMP_Group.SelectedObject = null;
                        UpdateListBox(HPTI_TPTI_Section.HPTIValueList, KMP_Group_ListBox);

                        KMP_Path_ListBox.Items.Clear();
                        propertyGrid_KMP_Path.SelectedObject = null;

                        KMP_Group_ListBox.SelectedIndex = KMP_Group_ListBox.Items.Count - 1;
                    }
                }
                if (KMPSectionComboBox.Text == "CheckPoint")
                {
                    int N = KMP_Group_ListBox.SelectedIndex;
                    if (N != -1)
                    {
                        var g = KMPViewportObject.Checkpoint_Rail[N];

                        HTK_3DES.KMP_3DCheckpointSystem.DeleteRailChk(render, g);
                        KMPViewportObject.Checkpoint_Rail.RemoveAt(N);

                        ReInputModelID(KMPViewportObject.Checkpoint_Rail, "Checkpoint_Left", "Checkpoint_Right");

                        HPKC_TPKC_Section.HPKCValueList.RemoveAt(N);
                        KMP_Group_ListBox.Items.RemoveAt(N);

                        ReInputID(HPKC_TPKC_Section.HPKCValueList, "GroupID");

                        for (int ReInputCount = 0; ReInputCount < HPKC_TPKC_Section.HPKCValueList.Count; ReInputCount++)
                        {
                            for (int i = 0; i < HPKC_TPKC_Section.HPKCValueList[ReInputCount].TPKCValueList.Count; i++)
                            {
                                SetProperty(HPKC_TPKC_Section.HPKCValueList[ReInputCount].TPKCValueList[i], ReInputCount, "Group_ID");
                            }
                        }

                        propertyGrid_KMP_Group.SelectedObject = null;
                        UpdateListBox(HPKC_TPKC_Section.HPKCValueList, KMP_Group_ListBox);

                        KMP_Path_ListBox.Items.Clear();
                        propertyGrid_KMP_Path.SelectedObject = null;

                        KMP_Group_ListBox.SelectedIndex = KMP_Group_ListBox.Items.Count - 1;
                    }
                }
                if (KMPSectionComboBox.Text == "Route")
                {
                    int N = KMP_Group_ListBox.SelectedIndex;
                    if (N != -1)
                    {
                        HTK_3DES.PathTools.Rail rail = KMPViewportObject.Routes_List[N];
                        HTK_3DES.PathTools.DeleteRail(render, rail);
                        KMPViewportObject.Routes_List.RemoveAt(N);

                        ReInputModelID(KMPViewportObject.Routes_List, "Routes");

                        ITOP_Section.ITOP_RouteList.RemoveAt(N);
                        KMP_Group_ListBox.Items.RemoveAt(N);

                        ReInputID(ITOP_Section.ITOP_RouteList, "GroupID");

                        for (int ReInputCount = 0; ReInputCount < ITOP_Section.ITOP_RouteList.Count; ReInputCount++)
                        {
                            for (int i = 0; i < ITOP_Section.ITOP_RouteList[ReInputCount].ITOP_PointList.Count; i++)
                            {
                                SetProperty(ITOP_Section.ITOP_RouteList[ReInputCount].ITOP_PointList[i], ReInputCount, "GroupID");
                            }
                        }

                        propertyGrid_KMP_Group.SelectedObject = null;
                        UpdateListBox(ITOP_Section.ITOP_RouteList, KMP_Group_ListBox);

                        KMP_Path_ListBox.Items.Clear();
                        propertyGrid_KMP_Path.SelectedObject = null;

                        KMP_Group_ListBox.SelectedIndex = KMP_Group_ListBox.Items.Count - 1;
                    }
                }
                if (KMPSectionComboBox.Text == "GlideRoutes")
                {
                    int N = KMP_Group_ListBox.SelectedIndex;
                    if (N != -1)
                    {
                        HTK_3DES.PathTools.Rail rail = KMPViewportObject.GlideRoute_Rail_List[N];
                        HTK_3DES.PathTools.DeleteRail(render, rail);
                        KMPViewportObject.GlideRoute_Rail_List.RemoveAt(N);

                        ReInputModelID(KMPViewportObject.GlideRoute_Rail_List, "GlideRoutes");

                        HPLG_TPLG_Section.HPLGValueList.RemoveAt(N);
                        KMP_Group_ListBox.Items.RemoveAt(N);

                        ReInputID(HPLG_TPLG_Section.HPLGValueList, "GroupID");

                        for (int ReInputCount = 0; ReInputCount < HPLG_TPLG_Section.HPLGValueList.Count; ReInputCount++)
                        {
                            for (int i = 0; i < HPLG_TPLG_Section.HPLGValueList[ReInputCount].TPLGValueList.Count; i++)
                            {
                                SetProperty(HPLG_TPLG_Section.HPLGValueList[ReInputCount].TPLGValueList[i], ReInputCount, "GroupID");
                            }
                        }

                        propertyGrid_KMP_Group.SelectedObject = null;
                        UpdateListBox(HPLG_TPLG_Section.HPLGValueList, KMP_Group_ListBox);

                        KMP_Path_ListBox.Items.Clear();
                        propertyGrid_KMP_Path.SelectedObject = null;

                        KMP_Group_ListBox.SelectedIndex = KMP_Group_ListBox.Items.Count - 1;
                    }
                }
            }
            if (tabControl1.SelectedIndex == 1)
            {
                if (KMPSectionComboBox.Text == "KartPoint")
                {
                    int N = KMP_Path_ListBox.SelectedIndex;
                    if (N != -1)
                    {
                        //Itemの削除
                        DeleteItem(TPTK_Section.TPTKValueList, KMPViewportObject.StartPosition_MV3DList, KMP_Path_ListBox, N);

                        //再度IDを入れる
                        ReInputID(TPTK_Section.TPTKValueList, "ID");
                        ReInputModelID(KMPViewportObject.StartPosition_MV3DList, "StartPosition");

                        propertyGrid_KMP_Path.SelectedObject = null;

                        //ListBoxの再描画
                        UpdateListBox(TPTK_Section.TPTKValueList, KMP_Path_ListBox);

                        KMP_Path_ListBox.SelectedIndex = KMP_Path_ListBox.Items.Count - 1;
                    }
                }
                if (KMPSectionComboBox.Text == "EnemyRoutes")
                {
                    if (KMP_Group_ListBox.SelectedIndex != -1)
                    {
                        int N = KMP_Path_ListBox.SelectedIndex;
                        if (N != -1)
                        {
                            HPNE_TPNE_Section.HPNEValueList[KMP_Group_ListBox.SelectedIndex].TPNEValueList.RemoveAt(N);

                            HTK_3DES.PathTools.Rail r = KMPViewportObject.EnemyRoute_Rail_List[KMP_Group_ListBox.SelectedIndex];
                            HTK_3DES.PathTools.DeleteRailPoint(render, r, N, 10.0, Colors.Orange, HTK_3DES.PathTools.RailType.Tube);

                            KMP_Path_ListBox.Items.RemoveAt(N);

                            ReInputID(HPNE_TPNE_Section.HPNEValueList[KMP_Group_ListBox.SelectedIndex].TPNEValueList, "ID");
                            propertyGrid_KMP_Path.SelectedObject = null;
                            UpdateListBox(HPNE_TPNE_Section.HPNEValueList[KMP_Group_ListBox.SelectedIndex].TPNEValueList, KMP_Path_ListBox);
                            KMP_Path_ListBox.SelectedIndex = KMP_Path_ListBox.Items.Count - 1;
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Groupを選択してください");
                    }
                }
                if (KMPSectionComboBox.Text == "ItemRoutes")
                {
                    if (KMP_Group_ListBox.SelectedIndex != -1)
                    {
                        int N = KMP_Path_ListBox.SelectedIndex;
                        if (N != -1)
                        {
                            HPTI_TPTI_Section.HPTIValueList[KMP_Group_ListBox.SelectedIndex].TPTIValueList.RemoveAt(N);

                            HTK_3DES.PathTools.Rail r = KMPViewportObject.ItemRoute_Rail_List[KMP_Group_ListBox.SelectedIndex];
                            HTK_3DES.PathTools.DeleteRailPoint(render, r, N, 10.0, Colors.Green, HTK_3DES.PathTools.RailType.Tube);

                            KMP_Path_ListBox.Items.RemoveAt(N);

                            ReInputID(HPTI_TPTI_Section.HPTIValueList[KMP_Group_ListBox.SelectedIndex].TPTIValueList, "ID");
                            propertyGrid_KMP_Path.SelectedObject = null;
                            UpdateListBox(HPTI_TPTI_Section.HPTIValueList[KMP_Group_ListBox.SelectedIndex].TPTIValueList, KMP_Path_ListBox);
                            KMP_Path_ListBox.SelectedIndex = KMP_Path_ListBox.Items.Count - 1;
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Group : Null");
                    }
                }
                if (KMPSectionComboBox.Text == "CheckPoint")
                {
                    if (KMP_Group_ListBox.SelectedIndex != -1)
                    {
                        int N = KMP_Path_ListBox.SelectedIndex;
                        if (N != -1)
                        {
                            HPKC_TPKC_Section.HPKCValueList[KMP_Group_ListBox.SelectedIndex].TPKCValueList.RemoveAt(N);

                            render.MainViewPort.Children.Remove(KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Line[N]);
                            KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Line.RemoveAt(N);

                            HTK_3DES.PathTools.Rail r = KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Left;
                            HTK_3DES.PathTools.DeleteRailPoint(render, r, N, 10.0, Colors.Green, HTK_3DES.PathTools.RailType.Line);

                            HTK_3DES.PathTools.Rail r2 = KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Right;
                            HTK_3DES.PathTools.DeleteRailPoint(render, r2, N, 10.0, Colors.Red, HTK_3DES.PathTools.RailType.Line);

                            KMP_Path_ListBox.Items.RemoveAt(N);

                            ReInputID(HPKC_TPKC_Section.HPKCValueList[KMP_Group_ListBox.SelectedIndex].TPKCValueList, "ID");
                            propertyGrid_KMP_Path.SelectedObject = null;
                            UpdateListBox(HPKC_TPKC_Section.HPKCValueList[KMP_Group_ListBox.SelectedIndex].TPKCValueList, KMP_Path_ListBox);
                            KMP_Path_ListBox.SelectedIndex = KMP_Path_ListBox.Items.Count - 1;
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Groupを選択してください");
                    }
                }
                if (KMPSectionComboBox.Text == "Obj")
                {
                    int N = KMP_Path_ListBox.SelectedIndex;
                    if (N != -1)
                    {
                        //Itemの削除
                        DeleteItem(JBOG_Section.JBOGValueList, KMPViewportObject.OBJ_MV3DList, KMP_Path_ListBox, N);

                        //再度IDを入れる
                        ReInputID(JBOG_Section.JBOGValueList, "ID");
                        ReInputModelID(KMPViewportObject.OBJ_MV3DList, "OBJ");

                        propertyGrid_KMP_Path.SelectedObject = null;

                        //ListBoxの再描画
                        UpdateListBox(JBOG_Section.JBOGValueList, KMP_Path_ListBox);

                        KMP_Path_ListBox.SelectedIndex = KMP_Path_ListBox.Items.Count - 1;
                    }
                }
                if (KMPSectionComboBox.Text == "Route")
                {
                    if (KMP_Group_ListBox.SelectedIndex != -1)
                    {
                        int N = KMP_Path_ListBox.SelectedIndex;
                        if (N != -1)
                        {
                            ITOP_Section.ITOP_RouteList[KMP_Group_ListBox.SelectedIndex].ITOP_PointList.RemoveAt(N);

                            HTK_3DES.PathTools.Rail r = KMPViewportObject.Routes_List[KMP_Group_ListBox.SelectedIndex];
                            HTK_3DES.PathTools.DeleteRailPoint(render, r, N, 10.0, Colors.Blue, HTK_3DES.PathTools.RailType.Tube);

                            KMP_Path_ListBox.Items.RemoveAt(N);

                            ReInputID(ITOP_Section.ITOP_RouteList[KMP_Group_ListBox.SelectedIndex].ITOP_PointList, "ID");
                            propertyGrid_KMP_Path.SelectedObject = null;
                            UpdateListBox(ITOP_Section.ITOP_RouteList[KMP_Group_ListBox.SelectedIndex].ITOP_PointList, KMP_Path_ListBox);
                            KMP_Path_ListBox.SelectedIndex = KMP_Path_ListBox.Items.Count - 1;
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Group : Null");
                    }
                }
                if (KMPSectionComboBox.Text == "Area")
                {
                    int N = KMP_Path_ListBox.SelectedIndex;
                    if (N != -1)
                    {
                        //Itemの削除
                        DeleteItem(AERA_Section.AERAValueList, KMPViewportObject.Area_MV3DList, KMP_Path_ListBox, N);

                        //再度IDを入れる
                        ReInputID(AERA_Section.AERAValueList, "ID");
                        ReInputModelID(KMPViewportObject.Area_MV3DList, "Area");

                        propertyGrid_KMP_Path.SelectedObject = null;

                        //ListBoxの再描画
                        UpdateListBox(AERA_Section.AERAValueList, KMP_Path_ListBox);

                        KMP_Path_ListBox.SelectedIndex = KMP_Path_ListBox.Items.Count - 1;
                    }
                }
                if (KMPSectionComboBox.Text == "Camera")
                {
                    int N = KMP_Path_ListBox.SelectedIndex;
                    if (N != -1)
                    {
                        //Itemの削除
                        DeleteItem(EMAC_Section.EMACValueList, KMPViewportObject.Camera_MV3DList, KMP_Path_ListBox, N);

                        //再度IDを入れる
                        ReInputID(EMAC_Section.EMACValueList, "ID");
                        ReInputModelID(KMPViewportObject.Camera_MV3DList, "Camera");

                        propertyGrid_KMP_Path.SelectedObject = null;

                        //ListBoxの再描画
                        UpdateListBox(EMAC_Section.EMACValueList, KMP_Path_ListBox);

                        KMP_Path_ListBox.SelectedIndex = KMP_Path_ListBox.Items.Count - 1;
                    }
                }
                if (KMPSectionComboBox.Text == "JugemPoint")
                {
                    int N = KMP_Path_ListBox.SelectedIndex;
                    if (N != -1)
                    {
                        //Itemの削除
                        DeleteItem(TPGJ_Section.TPGJValueList, KMPViewportObject.RespawnPoint_MV3DList, KMP_Path_ListBox, N);

                        //再度IDを入れる
                        ReInputID(TPGJ_Section.TPGJValueList, "ID");
                        ReInputModelID(KMPViewportObject.RespawnPoint_MV3DList, "RespawnPoint");

                        propertyGrid_KMP_Path.SelectedObject = null;

                        //ListBoxの再描画
                        UpdateListBox(TPGJ_Section.TPGJValueList, KMP_Path_ListBox);

                        KMP_Path_ListBox.SelectedIndex = KMP_Path_ListBox.Items.Count - 1;
                    }
                }
                if (KMPSectionComboBox.Text == "GlideRoutes")
                {
                    if (KMP_Group_ListBox.SelectedIndex != -1)
                    {
                        int N = KMP_Path_ListBox.SelectedIndex;
                        if (N != -1)
                        {
                            HPLG_TPLG_Section.HPLGValueList[KMP_Group_ListBox.SelectedIndex].TPLGValueList.RemoveAt(N);

                            HTK_3DES.PathTools.Rail r = KMPViewportObject.GlideRoute_Rail_List[KMP_Group_ListBox.SelectedIndex];
                            HTK_3DES.PathTools.DeleteRailPoint(render, r, N, 10.0, Colors.LightSkyBlue, HTK_3DES.PathTools.RailType.Tube);

                            KMP_Path_ListBox.Items.RemoveAt(N);

                            ReInputID(HPLG_TPLG_Section.HPLGValueList[KMP_Group_ListBox.SelectedIndex].TPLGValueList, "ID");
                            propertyGrid_KMP_Path.SelectedObject = null;
                            UpdateListBox(HPLG_TPLG_Section.HPLGValueList[KMP_Group_ListBox.SelectedIndex].TPLGValueList, KMP_Path_ListBox);
                            KMP_Path_ListBox.SelectedIndex = KMP_Path_ListBox.Items.Count - 1;
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("Group : Null");
                    }
                }
            }
        }

        private void KMPSectionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            KMP_Path_ListBox.Items.Clear();
            KMP_Group_ListBox.Items.Clear();

            if (KMPSectionComboBox.Text == "KartPoint")
            {
                KMP_Path_ListBox.Items.AddRange(TPTK_Section.TPTKValueList.ToArray());

                xXXXRouteExporterToolStripMenuItem.Text = "XXXX Route Exporter";
                xXXXRouteExporterToolStripMenuItem.Enabled = false;

                xXXXRouteImporterToolStripMenuItem.Text = "XXXX Route Importer";
                xXXXRouteImporterToolStripMenuItem.Enabled = false;
            }
            if (KMPSectionComboBox.Text == "EnemyRoutes")
            {
                KMP_Group_ListBox.Items.AddRange(HPNE_TPNE_Section.HPNEValueList.ToArray());

                xXXXRouteExporterToolStripMenuItem.Text = KMPSectionComboBox.Text + " Route Exporter";
                xXXXRouteExporterToolStripMenuItem.Enabled = true;

                xXXXRouteImporterToolStripMenuItem.Text = KMPSectionComboBox.Text + " Route Importer";
                xXXXRouteImporterToolStripMenuItem.Enabled = true;
            }
            if (KMPSectionComboBox.Text == "ItemRoutes")
            {
                KMP_Group_ListBox.Items.AddRange(HPTI_TPTI_Section.HPTIValueList.ToArray());

                xXXXRouteExporterToolStripMenuItem.Text = KMPSectionComboBox.Text + " Route Exporter";
                xXXXRouteExporterToolStripMenuItem.Enabled = true;

                xXXXRouteImporterToolStripMenuItem.Text = KMPSectionComboBox.Text + " Route Importer";
                xXXXRouteImporterToolStripMenuItem.Enabled = true;
            }
            if (KMPSectionComboBox.Text == "CheckPoint")
            {
                KMP_Group_ListBox.Items.AddRange(HPKC_TPKC_Section.HPKCValueList.ToArray());

                xXXXRouteExporterToolStripMenuItem.Text = "XXXX Route Exporter";
                xXXXRouteExporterToolStripMenuItem.Enabled = false;

                xXXXRouteImporterToolStripMenuItem.Text = "XXXX Route Importer";
                xXXXRouteImporterToolStripMenuItem.Enabled = false;
            }
            if (KMPSectionComboBox.Text == "Obj")
            {
                KMP_Path_ListBox.Items.AddRange(JBOG_Section.JBOGValueList.ToArray());

                xXXXRouteExporterToolStripMenuItem.Text = "XXXX Route Exporter";
                xXXXRouteExporterToolStripMenuItem.Enabled = false;

                xXXXRouteImporterToolStripMenuItem.Text = "XXXX Route Importer";
                xXXXRouteImporterToolStripMenuItem.Enabled = false;
            }
            if (KMPSectionComboBox.Text == "Route")
            {
                KMP_Group_ListBox.Items.AddRange(ITOP_Section.ITOP_RouteList.ToArray());

                xXXXRouteExporterToolStripMenuItem.Text = "XXXX Route Exporter";
                xXXXRouteExporterToolStripMenuItem.Enabled = false;

                xXXXRouteImporterToolStripMenuItem.Text = "XXXX Route Importer";
                xXXXRouteImporterToolStripMenuItem.Enabled = false;
            }
            if (KMPSectionComboBox.Text == "Area")
            {
                KMP_Path_ListBox.Items.AddRange(AERA_Section.AERAValueList.ToArray());

                xXXXRouteExporterToolStripMenuItem.Text = "XXXX Route Exporter";
                xXXXRouteExporterToolStripMenuItem.Enabled = false;

                xXXXRouteImporterToolStripMenuItem.Text = "XXXX Route Importer";
                xXXXRouteImporterToolStripMenuItem.Enabled = false;
            }
            if (KMPSectionComboBox.Text == "Camera")
            {
                KMP_Path_ListBox.Items.AddRange(EMAC_Section.EMACValueList.ToArray());

                xXXXRouteExporterToolStripMenuItem.Text = "XXXX Route Exporter";
                xXXXRouteExporterToolStripMenuItem.Enabled = false;

                xXXXRouteImporterToolStripMenuItem.Text = "XXXX Route Importer";
                xXXXRouteImporterToolStripMenuItem.Enabled = false;
            }
            if (KMPSectionComboBox.Text == "JugemPoint")
            {
                KMP_Path_ListBox.Items.AddRange(TPGJ_Section.TPGJValueList.ToArray());

                xXXXRouteExporterToolStripMenuItem.Text = "XXXX Route Exporter";
                xXXXRouteExporterToolStripMenuItem.Enabled = false;

                xXXXRouteImporterToolStripMenuItem.Text = "XXXX Route Importer";
                xXXXRouteImporterToolStripMenuItem.Enabled = false;
            }
            if (KMPSectionComboBox.Text == "GlideRoutes")
            {
                KMP_Group_ListBox.Items.AddRange(HPLG_TPLG_Section.HPLGValueList.ToArray());

                xXXXRouteExporterToolStripMenuItem.Text = KMPSectionComboBox.Text + " Route Exporter";
                xXXXRouteExporterToolStripMenuItem.Enabled = true;

                xXXXRouteImporterToolStripMenuItem.Text = KMPSectionComboBox.Text + " Route Importer";
                xXXXRouteImporterToolStripMenuItem.Enabled = true;
            }

            outputXXXXAsXmlToolStripMenuItem.Text = "Output " + KMPSectionComboBox.Text + " as Xml";
            inputXmlAsXXXXToolStripMenuItem.Text = "Input Xml as " + KMPSectionComboBox.Text; 
        }

        private void KMP_Group_ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (KMP_Group_ListBox.SelectedIndex != -1)
            {
                if (KMPSectionComboBox.SelectedIndex == -1) return;
                if (KMPSectionComboBox.Text == "EnemyRoutes")
                {
                    KMP_Path_ListBox.Items.Clear();
                    KMP_Path_ListBox.Items.AddRange(HPNE_TPNE_Section.HPNEValueList[KMP_Group_ListBox.SelectedIndex].TPNEValueList.ToArray());
                    propertyGrid_KMP_Group.SelectedObject = HPNE_TPNE_Section.HPNEValueList[KMP_Group_ListBox.SelectedIndex];
                }
                if (KMPSectionComboBox.Text == "ItemRoutes")
                {
                    KMP_Path_ListBox.Items.Clear();
                    KMP_Path_ListBox.Items.AddRange(HPTI_TPTI_Section.HPTIValueList[KMP_Group_ListBox.SelectedIndex].TPTIValueList.ToArray());
                    propertyGrid_KMP_Group.SelectedObject = HPTI_TPTI_Section.HPTIValueList[KMP_Group_ListBox.SelectedIndex];
                }
                if (KMPSectionComboBox.Text == "CheckPoint")
                {
                    KMP_Path_ListBox.Items.Clear();
                    KMP_Path_ListBox.Items.AddRange(HPKC_TPKC_Section.HPKCValueList[KMP_Group_ListBox.SelectedIndex].TPKCValueList.ToArray());
                    propertyGrid_KMP_Group.SelectedObject = HPKC_TPKC_Section.HPKCValueList[KMP_Group_ListBox.SelectedIndex];
                }
                if (KMPSectionComboBox.Text == "Route")
                {
                    KMP_Path_ListBox.Items.Clear();
                    KMP_Path_ListBox.Items.AddRange(ITOP_Section.ITOP_RouteList[KMP_Group_ListBox.SelectedIndex].ITOP_PointList.ToArray());
                    propertyGrid_KMP_Group.SelectedObject = ITOP_Section.ITOP_RouteList[KMP_Group_ListBox.SelectedIndex];
                }
                if (KMPSectionComboBox.Text == "GlideRoutes")
                {
                    KMP_Path_ListBox.Items.Clear();
                    KMP_Path_ListBox.Items.AddRange(HPLG_TPLG_Section.HPLGValueList[KMP_Group_ListBox.SelectedIndex].TPLGValueList.ToArray());
                    propertyGrid_KMP_Group.SelectedObject = HPLG_TPLG_Section.HPLGValueList[KMP_Group_ListBox.SelectedIndex];
                }
            }
        }

        private void KMP_Path_ListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (KMP_Path_ListBox.SelectedIndex == -1) return;
            if (KMPSectionComboBox.Text == "KartPoint")
            {
                propertyGrid_KMP_Path.SelectedObject = TPTK_Section.TPTKValueList[KMP_Path_ListBox.SelectedIndex];
            }
            if (KMPSectionComboBox.Text == "EnemyRoutes")
            {
                propertyGrid_KMP_Path.SelectedObject = HPNE_TPNE_Section.HPNEValueList[KMP_Group_ListBox.SelectedIndex].TPNEValueList[KMP_Path_ListBox.SelectedIndex];
            }
            if (KMPSectionComboBox.Text == "ItemRoutes")
            {
                propertyGrid_KMP_Path.SelectedObject = HPTI_TPTI_Section.HPTIValueList[KMP_Group_ListBox.SelectedIndex].TPTIValueList[KMP_Path_ListBox.SelectedIndex];
            }
            if (KMPSectionComboBox.Text == "CheckPoint")
            {
                propertyGrid_KMP_Path.SelectedObject = HPKC_TPKC_Section.HPKCValueList[KMP_Group_ListBox.SelectedIndex].TPKCValueList[KMP_Path_ListBox.SelectedIndex];
            }
            if (KMPSectionComboBox.Text == "Obj")
            {
                propertyGrid_KMP_Path.SelectedObject = JBOG_Section.JBOGValueList[KMP_Path_ListBox.SelectedIndex];
            }
            if (KMPSectionComboBox.Text == "Route")
            {
                propertyGrid_KMP_Path.SelectedObject = ITOP_Section.ITOP_RouteList[KMP_Group_ListBox.SelectedIndex].ITOP_PointList[KMP_Path_ListBox.SelectedIndex];
            }
            if (KMPSectionComboBox.Text == "Area")
            {
                propertyGrid_KMP_Path.SelectedObject = AERA_Section.AERAValueList[KMP_Path_ListBox.SelectedIndex];
            }
            if (KMPSectionComboBox.Text == "Camera")
            {
                propertyGrid_KMP_Path.SelectedObject = EMAC_Section.EMACValueList[KMP_Path_ListBox.SelectedIndex];
            }
            if (KMPSectionComboBox.Text == "JugemPoint")
            {
                propertyGrid_KMP_Path.SelectedObject = TPGJ_Section.TPGJValueList[KMP_Path_ListBox.SelectedIndex];
            }
            if (KMPSectionComboBox.Text == "GlideRoutes")
            {
                propertyGrid_KMP_Path.SelectedObject = HPLG_TPLG_Section.HPLGValueList[KMP_Group_ListBox.SelectedIndex].TPLGValueList[KMP_Path_ListBox.SelectedIndex];
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            double p = Convert.ToDouble((textBox1.Text != "") ? textBox1.Text : "0");

            for (int Count = 0; Count < KMPViewportObject.Checkpoint_Rail.Count; Count++)
            {
                for (int CP_DLine_L = 0; CP_DLine_L < KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Left.MV3D_List.Count; CP_DLine_L++)
                {
                    double SX = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Left.MV3D_List[CP_DLine_L].Content.Transform.Value.M11;
                    double SY = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Left.MV3D_List[CP_DLine_L].Content.Transform.Value.M22;
                    double SZ = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Left.MV3D_List[CP_DLine_L].Content.Transform.Value.M33;

                    double TX = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Left.MV3D_List[CP_DLine_L].Content.Transform.Value.OffsetX;
                    double TY = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Left.MV3D_List[CP_DLine_L].Content.Transform.Value.OffsetY;
                    double TZ = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Left.MV3D_List[CP_DLine_L].Content.Transform.Value.OffsetZ;

                    HTK_3DES.TSRSystem.Transform t = new HTK_3DES.TSRSystem.Transform
                    {
                        Rotate3D = new Vector3D(0, 0, 0),
                        Scale3D = new Vector3D(SX, SY, SZ),
                        Translate3D = new Vector3D(TX, p, TZ)
                    };

                    HTK_3DES.TransformMV3D.Transform_MV3D(t, KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Left.MV3D_List[CP_DLine_L], HTK_3DES.TSRSystem.RotationSetting.Radian, true, 1);
                }

                for (int CP_DLine_R = 0; CP_DLine_R < KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Right.MV3D_List.Count; CP_DLine_R++)
                {
                    double SX = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Right.MV3D_List[CP_DLine_R].Content.Transform.Value.M11;
                    double SY = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Right.MV3D_List[CP_DLine_R].Content.Transform.Value.M22;
                    double SZ = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Right.MV3D_List[CP_DLine_R].Content.Transform.Value.M33;

                    double TX = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Right.MV3D_List[CP_DLine_R].Content.Transform.Value.OffsetX;
                    double TY = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Right.MV3D_List[CP_DLine_R].Content.Transform.Value.OffsetY;
                    double TZ = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Right.MV3D_List[CP_DLine_R].Content.Transform.Value.OffsetZ;

                    HTK_3DES.TSRSystem.Transform t = new HTK_3DES.TSRSystem.Transform
                    {
                        Rotate3D = new Vector3D(0, 0, 0),
                        Scale3D = new Vector3D(SX, SY, SZ),
                        Translate3D = new Vector3D(TX, p, TZ)
                    };

                    HTK_3DES.TransformMV3D.Transform_MV3D(t, KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Right.MV3D_List[CP_DLine_R], HTK_3DES.TSRSystem.RotationSetting.Radian, true, 1);
                }

                for (int CP_Dline = 0; CP_Dline < KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Line.Count; CP_Dline++)
                {
                    var DividingLine1 = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Line[CP_Dline].Points[0];
                    KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Line[CP_Dline].Points[0] = new Point3D(DividingLine1.X, p, DividingLine1.Z);

                    var DividingLine2 = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Line[CP_Dline].Points[1];
                    KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Line[CP_Dline].Points[1] = new Point3D(DividingLine2.X, p, DividingLine2.Z);
                }

                for (int CP_RLine_L = 0; CP_RLine_L < KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Left.LV3D_List.Count; CP_RLine_L++)
                {
                    var RailLineLeft1 = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Left.LV3D_List[CP_RLine_L].Points[0];
                    KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Left.LV3D_List[CP_RLine_L].Points[0] = new Point3D(RailLineLeft1.X, p, RailLineLeft1.Z);

                    var RailLineLeft2 = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Left.LV3D_List[CP_RLine_L].Points[1];
                    KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Left.LV3D_List[CP_RLine_L].Points[1] = new Point3D(RailLineLeft2.X, p, RailLineLeft2.Z);
                }

                for (int CP_DLine_R = 0; CP_DLine_R < KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Right.LV3D_List.Count; CP_DLine_R++)
                {
                    var RailLineRight1 = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Right.LV3D_List[CP_DLine_R].Points[0];
                    KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Right.LV3D_List[CP_DLine_R].Points[0] = new Point3D(RailLineRight1.X, p, RailLineRight1.Z);

                    var RailLineRight2 = KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Right.LV3D_List[CP_DLine_R].Points[1];
                    KMPViewportObject.Checkpoint_Rail[Count].Checkpoint_Right.LV3D_List[CP_DLine_R].Points[1] = new Point3D(RailLineRight2.X, p, RailLineRight2.Z);
                }
            }
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (textBox1.Text == "") textBox1.Text = "0";
        }

        private void objFlowXmlEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists("ObjFlow.bin") == true)
            {
                if (File.Exists("ObjFlowData.xml") == true)
                {
                    ObjFlowXmlEditor objFlowXmlEditor = new ObjFlowXmlEditor();
                    objFlowXmlEditor.Show();
                }
                if (File.Exists("ObjFlowData.xml") == false)
                {
                    string MText = "ObjFlowXmlEditorを使用するにはObjFlowData.Xmlが必要です。\r\nObjFlow.binを使用してObjFlowData.xmlを作成しますか?";
                    string Caption = "確認";
                    MessageBoxButton messageBoxButton = MessageBoxButton.YesNo;

                    DialogResult dialogResult = (DialogResult)System.Windows.MessageBox.Show(MText, Caption, messageBoxButton);
                    if(dialogResult == DialogResult.Yes)
                    {
                        List<KMPs.KMPHelper.ObjFlowReader.ObjFlowValue> objFlowValues = KMPs.KMPHelper.ObjFlowReader.Read("ObjFlow.bin");
                        KMPs.KMPHelper.ObjFlowReader.CreateXml(objFlowValues, "KMPObjectFlow", "KMP_OBJ\\OBJ\\OBJ.obj", "ObjFlowData.xml");

                        ObjFlowXmlEditor objFlowXmlEditor = new ObjFlowXmlEditor();
                        objFlowXmlEditor.Show();
                    }
                    if (dialogResult == DialogResult.No)
                    {
                        System.Windows.MessageBox.Show("ObjFlowData.xmlの作成を中止しました");
                        return;
                    }
                }
            }
            if (File.Exists("ObjFlow.bin") == false) System.Windows.MessageBox.Show("[ObjFlowXmlEditor]\r\nObjFlow.bin : null\r\nObjFlow.binがこのプログラムと同じディレクトリ内に存在しません。\r\nObjFlow.binをこのプログラムと同じディレクトリに配置してください。\r\nObjFlow.binは[RaceCommon.szs]に格納されています。", "Error");
        }

        private void propertyGrid_KMP_Path_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (KMPSectionComboBox.Text == "KartPoint")
            {
                KMPPropertyGridSettings.TPTK_Section.TPTKValue GetTPTKValue = TPTK_Section.TPTKValueList[KMP_Path_ListBox.SelectedIndex];

                HTK_3DES.TSRSystem.Transform t = new HTK_3DES.TSRSystem.Transform
                {
                    Rotate3D = new Vector3D(GetTPTKValue.Rotate_Value.X, GetTPTKValue.Rotate_Value.Y, GetTPTKValue.Rotate_Value.Z),
                    Scale3D = new Vector3D(10, 10, 10),
                    Translate3D = new Vector3D(GetTPTKValue.Position_Value.X, GetTPTKValue.Position_Value.Y, GetTPTKValue.Position_Value.Z)
                };

                HTK_3DES.TransformMV3D.Transform_MV3D(t, KMPViewportObject.StartPosition_MV3DList[KMP_Path_ListBox.SelectedIndex], HTK_3DES.TSRSystem.RotationSetting.Angle, true);
            }
            if (KMPSectionComboBox.Text == "EnemyRoutes")
            {
                if (KMP_Group_ListBox.SelectedIndex != -1)
                {
                    KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue.TPNEValue GetTPNEValue = HPNE_TPNE_Section.HPNEValueList[KMP_Group_ListBox.SelectedIndex].TPNEValueList[KMP_Path_ListBox.SelectedIndex];

                    HTK_3DES.TSRSystem.Transform t = new HTK_3DES.TSRSystem.Transform
                    {
                        Rotate3D = new Vector3D(0, 0, 0),
                        Scale3D = new Vector3D(GetTPNEValue.Control * 100, GetTPNEValue.Control * 100, GetTPNEValue.Control * 100),
                        Translate3D = new Vector3D(GetTPNEValue.Positions.X, GetTPNEValue.Positions.Y, GetTPNEValue.Positions.Z)
                    };

                    //パスの形を変更
                    HTK_3DES.PathTools.Rail rail = KMPViewportObject.EnemyRoute_Rail_List[KMP_Group_ListBox.SelectedIndex];
                    if (rail.TV3D_List.Count != 0) HTK_3DES.PathTools.MoveRails(KMP_Path_ListBox.SelectedIndex, t.Translate3D, rail.TV3D_List);

                    HTK_3DES.TransformMV3D.Transform_MV3D(t, KMPViewportObject.EnemyRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].MV3D_List[KMP_Path_ListBox.SelectedIndex], HTK_3DES.TSRSystem.RotationSetting.Angle, true);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Group : Null");
                }
            }
            if (KMPSectionComboBox.Text == "ItemRoutes")
            {
                if (KMP_Group_ListBox.SelectedIndex != -1)
                {
                    KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue.TPTIValue GetTPTIValue = HPTI_TPTI_Section.HPTIValueList[KMP_Group_ListBox.SelectedIndex].TPTIValueList[KMP_Path_ListBox.SelectedIndex];

                    HTK_3DES.TSRSystem.Transform t = new HTK_3DES.TSRSystem.Transform
                    {
                        Rotate3D = new Vector3D(0, 0, 0),
                        Scale3D = new Vector3D(GetTPTIValue.TPTI_PointSize * 100, GetTPTIValue.TPTI_PointSize * 100, GetTPTIValue.TPTI_PointSize * 100),
                        Translate3D = new Vector3D(GetTPTIValue.TPTI_Positions.X, GetTPTIValue.TPTI_Positions.Y, GetTPTIValue.TPTI_Positions.Z)
                    };

                    //パスの形を変更
                    HTK_3DES.PathTools.Rail rail = KMPViewportObject.ItemRoute_Rail_List[KMP_Group_ListBox.SelectedIndex];
                    if (rail.TV3D_List.Count != 0) HTK_3DES.PathTools.MoveRails(KMP_Path_ListBox.SelectedIndex, t.Translate3D, rail.TV3D_List);

                    HTK_3DES.TransformMV3D.Transform_MV3D(t, KMPViewportObject.ItemRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].MV3D_List[KMP_Path_ListBox.SelectedIndex], HTK_3DES.TSRSystem.RotationSetting.Angle, true);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Group : Null");
                }
            }
            if (KMPSectionComboBox.Text == "CheckPoint")
            {
                if (KMP_Group_ListBox.SelectedIndex != -1)
                {
                    #region Point Left
                    KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue GetTPKCValue_Left = HPKC_TPKC_Section.HPKCValueList[KMP_Group_ListBox.SelectedIndex].TPKCValueList[KMP_Path_ListBox.SelectedIndex];

                    HTK_3DES.TSRSystem.Transform t_Left = new HTK_3DES.TSRSystem.Transform
                    {
                        Rotate3D = new Vector3D(0, 0, 0),
                        Scale3D = new Vector3D(50, 50, 50),
                        Translate3D = new Vector3D(GetTPKCValue_Left.Position_2D_Left.X, float.Parse(textBox1.Text), GetTPKCValue_Left.Position_2D_Left.Y)
                    };

                    HTK_3DES.TransformMV3D.Transform_MV3D(t_Left, KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Left.MV3D_List[KMP_Path_ListBox.SelectedIndex], HTK_3DES.TSRSystem.RotationSetting.Angle, true);

                    //パスの形を変更(Green)
                    HTK_3DES.KMP_3DCheckpointSystem.Checkpoint checkpoint_Left = KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex];
                    if (checkpoint_Left.Checkpoint_Left.LV3D_List.Count != 0) HTK_3DES.PathTools.MoveRails(KMP_Path_ListBox.SelectedIndex, t_Left.Translate3D, checkpoint_Left.Checkpoint_Left.LV3D_List);
                    KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Line[KMP_Path_ListBox.SelectedIndex].Points[0] = t_Left.Translate3D.ToPoint3D();
                    #endregion

                    #region Point_Right

                    KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue.TPKCValue GetTPKCValue_Right = HPKC_TPKC_Section.HPKCValueList[KMP_Group_ListBox.SelectedIndex].TPKCValueList[KMP_Path_ListBox.SelectedIndex];

                    HTK_3DES.TSRSystem.Transform t_Right = new HTK_3DES.TSRSystem.Transform
                    {
                        Rotate3D = new Vector3D(0, 0, 0),
                        Scale3D = new Vector3D(50, 50, 50),
                        Translate3D = new Vector3D(GetTPKCValue_Right.Position_2D_Right.X, float.Parse(textBox1.Text), GetTPKCValue_Right.Position_2D_Right.Y)
                    };

                    HTK_3DES.TransformMV3D.Transform_MV3D(t_Right, KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Right.MV3D_List[KMP_Path_ListBox.SelectedIndex], HTK_3DES.TSRSystem.RotationSetting.Angle, true);

                    //パスの形を変更(Red)
                    HTK_3DES.KMP_3DCheckpointSystem.Checkpoint checkpoint_Right = KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex];
                    if (checkpoint_Right.Checkpoint_Right.LV3D_List.Count != 0) HTK_3DES.PathTools.MoveRails(KMP_Path_ListBox.SelectedIndex, t_Right.Translate3D, checkpoint_Right.Checkpoint_Right.LV3D_List);
                    KMPViewportObject.Checkpoint_Rail[KMP_Group_ListBox.SelectedIndex].Checkpoint_Line[KMP_Path_ListBox.SelectedIndex].Points[1] = t_Right.Translate3D.ToPoint3D();
                    #endregion
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Group : Null");
                }
            }
            if (KMPSectionComboBox.Text == "Obj")
            {
                render.MainViewPort.Children.Remove(KMPViewportObject.OBJ_MV3DList[KMP_Path_ListBox.SelectedIndex]);
                KMPViewportObject.OBJ_MV3DList.Remove(KMPViewportObject.OBJ_MV3DList[KMP_Path_ListBox.SelectedIndex]);

                KMPPropertyGridSettings.JBOG_section.JBOGValue GetJBOGValue = JBOG_Section.JBOGValueList[KMP_Path_ListBox.SelectedIndex];

                KMPs.KMPHelper.ObjFlowReader.ObjFlowXmlToObject objFlowXmlToObject_FindName = KMPs.KMPHelper.ObjFlowReader.ReadObjFlowXml("ObjFlowData.xml");
                string ObjectName = objFlowXmlToObject_FindName.ObjFlows.Find(x => x.ObjectID == GetJBOGValue.ObjectID).ObjectName;
                JBOG_Section.JBOGValueList[KMP_Path_ListBox.SelectedIndex].ObjectName = ObjectName;

                #region Add Model(OBJ)
                HTK_3DES.TSRSystem.Transform_Value OBJ_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                {
                    Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                    {
                        X = GetJBOGValue.Positions.X,
                        Y = GetJBOGValue.Positions.Y,
                        Z = GetJBOGValue.Positions.Z
                    },
                    Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                    {
                        X = GetJBOGValue.Scales.X * 2,
                        Y = GetJBOGValue.Scales.Y * 2,
                        Z = GetJBOGValue.Scales.Z * 2
                    },
                    Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                    {
                        X = GetJBOGValue.Rotations.X,
                        Y = GetJBOGValue.Rotations.Y,
                        Z = GetJBOGValue.Rotations.Z
                    }
                };

                KMPs.KMPHelper.ObjFlowReader.ObjFlowXmlToObject objFlowXmlToObject = KMPs.KMPHelper.ObjFlowReader.ReadObjFlowXml("ObjFlowData.xml");
                string Path = objFlowXmlToObject.ObjFlows.Find(x => x.ObjectID == GetJBOGValue.ObjectID).Path;
                ModelVisual3D dv3D_OBJ = HTK_3DES.TSRSystem.OBJReader(Path);

                //モデルの名前と番号を文字列に格納(情報化)
                HTK_3DES.TSRSystem.SetString_MV3D(dv3D_OBJ, "OBJ " + KMP_Path_ListBox.SelectedIndex + " " + -1);

                HTK_3DES.TransformMV3D.Transform_MV3D(OBJ_transform_Value, dv3D_OBJ, HTK_3DES.TSRSystem.RotationSetting.Angle, true);

                KMPViewportObject.OBJ_MV3DList.Insert(KMP_Path_ListBox.SelectedIndex, dv3D_OBJ);

                render.MainViewPort.Children.Insert(KMP_Path_ListBox.SelectedIndex, dv3D_OBJ);
                #endregion

                KMP_Path_ListBox.Items.Clear();
                KMP_Path_ListBox.Items.AddRange(JBOG_Section.JBOGValueList.ToArray());
                KMP_Path_ListBox.SelectedIndex = int.Parse(dv3D_OBJ.GetName().Split(' ')[1]);
            }
            if (KMPSectionComboBox.Text == "Route")
            {
                if (KMP_Group_ListBox.SelectedIndex != -1)
                {
                    KMPPropertyGridSettings.ITOP_Section.ITOP_Route.ITOP_Point GetITOPValue = ITOP_Section.ITOP_RouteList[KMP_Group_ListBox.SelectedIndex].ITOP_PointList[KMP_Path_ListBox.SelectedIndex];

                    HTK_3DES.TSRSystem.Transform t = new HTK_3DES.TSRSystem.Transform
                    {
                        Rotate3D = new Vector3D(0, 0, 0),
                        Scale3D = new Vector3D(10, 10, 10),
                        Translate3D = new Vector3D(GetITOPValue.Positions.X, GetITOPValue.Positions.Y, GetITOPValue.Positions.Z)
                    };

                    //パスの形を変更
                    HTK_3DES.PathTools.Rail rail = KMPViewportObject.Routes_List[KMP_Group_ListBox.SelectedIndex];
                    if (rail.TV3D_List.Count != 0) HTK_3DES.PathTools.MoveRails(KMP_Path_ListBox.SelectedIndex, t.Translate3D, rail.TV3D_List);

                    HTK_3DES.TransformMV3D.Transform_MV3D(t, KMPViewportObject.Routes_List[KMP_Group_ListBox.SelectedIndex].MV3D_List[KMP_Path_ListBox.SelectedIndex], HTK_3DES.TSRSystem.RotationSetting.Angle, true);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Group : Null");
                }
            }
            if (KMPSectionComboBox.Text == "Area")
            {
                render.MainViewPort.Children.Remove(KMPViewportObject.Area_MV3DList[KMP_Path_ListBox.SelectedIndex]);
                KMPViewportObject.Area_MV3DList.Remove(KMPViewportObject.Area_MV3DList[KMP_Path_ListBox.SelectedIndex]);

                KMPPropertyGridSettings.AERA_Section.AERAValue GetAERAValue = AERA_Section.AERAValueList[KMP_Path_ListBox.SelectedIndex];

                #region Add Model(OBJ)
                HTK_3DES.TSRSystem.Transform_Value Area_transform_Value = new HTK_3DES.TSRSystem.Transform_Value
                {
                    Translate_Value = new HTK_3DES.TSRSystem.Transform_Value.Translate
                    {
                        X = GetAERAValue.Positions.X,
                        Y = GetAERAValue.Positions.Y,
                        Z = GetAERAValue.Positions.Z
                    },
                    Scale_Value = new HTK_3DES.TSRSystem.Transform_Value.Scale
                    {
                        X = GetAERAValue.Scales.X * 1000,
                        Y = GetAERAValue.Scales.Y * 1000,
                        Z = GetAERAValue.Scales.Z * 1000
                    },
                    Rotate_Value = new HTK_3DES.TSRSystem.Transform_Value.Rotate
                    {
                        X = GetAERAValue.Rotations.X,
                        Y = GetAERAValue.Rotations.Y,
                        Z = GetAERAValue.Rotations.Z
                    }
                };

                ModelVisual3D dv3D_AreaOBJ = null;
                if (GetAERAValue.AreaModeSettings.AreaModeValue == 0) dv3D_AreaOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Area\\Area_Box\\Area_Box.obj");
                if (GetAERAValue.AreaModeSettings.AreaModeValue == 1) dv3D_AreaOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Area\\Area_Cylinder\\Area_Cylinder.obj");
                if (GetAERAValue.AreaModeSettings.AreaModeValue > 1) dv3D_AreaOBJ = HTK_3DES.TSRSystem.OBJReader("KMP_OBJ\\Area\\Area_Box\\Area_Box.obj");

                //モデルの名前と番号を文字列に格納(情報化)
                HTK_3DES.TSRSystem.SetString_MV3D(dv3D_AreaOBJ, "Area " + KMP_Path_ListBox.SelectedIndex + " " + -1);

                HTK_3DES.TransformMV3D.Transform_MV3D(Area_transform_Value, dv3D_AreaOBJ, HTK_3DES.TSRSystem.RotationSetting.Angle, true);

                KMPViewportObject.Area_MV3DList.Insert(KMP_Path_ListBox.SelectedIndex, dv3D_AreaOBJ);

                render.MainViewPort.Children.Insert(KMP_Path_ListBox.SelectedIndex, dv3D_AreaOBJ);
                #endregion
            }
            if (KMPSectionComboBox.Text == "Camera")
            {
                KMPPropertyGridSettings.EMAC_Section.EMACValue GetAERAValue = EMAC_Section.EMACValueList[KMP_Path_ListBox.SelectedIndex];

                HTK_3DES.TSRSystem.Transform t = new HTK_3DES.TSRSystem.Transform
                {
                    Rotate3D = new Vector3D(GetAERAValue.Rotations.X, GetAERAValue.Rotations.Y, GetAERAValue.Rotations.Z),
                    Scale3D = new Vector3D(10, 10, 10),
                    Translate3D = new Vector3D(GetAERAValue.Positions.X, GetAERAValue.Positions.Y, GetAERAValue.Positions.Z)
                };

                HTK_3DES.TransformMV3D.Transform_MV3D(t, KMPViewportObject.Camera_MV3DList[KMP_Path_ListBox.SelectedIndex], HTK_3DES.TSRSystem.RotationSetting.Angle, true);
            }
            if (KMPSectionComboBox.Text == "JugemPoint")
            {
                KMPPropertyGridSettings.TPGJ_Section.TPGJValue GetTPGJValue = TPGJ_Section.TPGJValueList[KMP_Path_ListBox.SelectedIndex];

                HTK_3DES.TSRSystem.Transform t = new HTK_3DES.TSRSystem.Transform
                {
                    Rotate3D = new Vector3D(GetTPGJValue.Rotations.X, GetTPGJValue.Rotations.Y, GetTPGJValue.Rotations.Z),
                    Scale3D = new Vector3D(10, 10, 10),
                    Translate3D = new Vector3D(GetTPGJValue.Positions.X, GetTPGJValue.Positions.Y, GetTPGJValue.Positions.Z)
                };

                HTK_3DES.TransformMV3D.Transform_MV3D(t, KMPViewportObject.RespawnPoint_MV3DList[KMP_Path_ListBox.SelectedIndex], HTK_3DES.TSRSystem.RotationSetting.Angle, true);
            }
            if (KMPSectionComboBox.Text == "GlideRoutes")
            {
                if (KMP_Group_ListBox.SelectedIndex != -1)
                {
                    KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue.TPLGValue GetTPLGValue = HPLG_TPLG_Section.HPLGValueList[KMP_Group_ListBox.SelectedIndex].TPLGValueList[KMP_Path_ListBox.SelectedIndex];

                    HTK_3DES.TSRSystem.Transform t = new HTK_3DES.TSRSystem.Transform
                    {
                        Rotate3D = new Vector3D(0, 0, 0),
                        Scale3D = new Vector3D(GetTPLGValue.TPLG_PointScaleValue * 100, GetTPLGValue.TPLG_PointScaleValue * 100, GetTPLGValue.TPLG_PointScaleValue * 100),
                        Translate3D = new Vector3D(GetTPLGValue.Positions.X, GetTPLGValue.Positions.Y, GetTPLGValue.Positions.Z)
                    };

                    //パスの形を変更
                    HTK_3DES.PathTools.Rail rail = KMPViewportObject.GlideRoute_Rail_List[KMP_Group_ListBox.SelectedIndex];
                    if (rail.TV3D_List.Count != 0) HTK_3DES.PathTools.MoveRails(KMP_Path_ListBox.SelectedIndex, t.Translate3D, rail.TV3D_List);

                    HTK_3DES.TransformMV3D.Transform_MV3D(t, KMPViewportObject.GlideRoute_Rail_List[KMP_Group_ListBox.SelectedIndex].MV3D_List[KMP_Path_ListBox.SelectedIndex], HTK_3DES.TSRSystem.RotationSetting.Angle, true);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Group : Null");
                }
            }
        }

        private void closeKMPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TPTK_Section = null;
            HPNE_TPNE_Section = null;
            HPTI_TPTI_Section = null;
            HPKC_TPKC_Section = null;
            JBOG_Section = null;
            ITOP_Section = null;
            AERA_Section = null;
            EMAC_Section = null;
            TPGJ_Section = null;
            IGTS_Section = null;
            HPLG_TPLG_Section = null;

            for (int del = 0; del < KMPViewportObject.StartPosition_MV3DList.Count; del++) render.MainViewPort.Children.Remove(KMPViewportObject.StartPosition_MV3DList[del]);
            KMPViewportObject.StartPosition_MV3DList.Clear();

            for (int del = 0; del < KMPViewportObject.EnemyRoute_Rail_List.Count; del++) HTK_3DES.PathTools.DeleteRail(render, KMPViewportObject.EnemyRoute_Rail_List[del]);
            KMPViewportObject.EnemyRoute_Rail_List.Clear();

            for (int del = 0; del < KMPViewportObject.ItemRoute_Rail_List.Count; del++) HTK_3DES.PathTools.DeleteRail(render, KMPViewportObject.ItemRoute_Rail_List[del]);
            KMPViewportObject.ItemRoute_Rail_List.Clear();

            for (int del = 0; del < KMPViewportObject.Checkpoint_Rail.Count; del++) HTK_3DES.KMP_3DCheckpointSystem.DeleteRailChk(render, KMPViewportObject.Checkpoint_Rail[del]);
            KMPViewportObject.ItemRoute_Rail_List.Clear();

            for (int del = 0; del < KMPViewportObject.OBJ_MV3DList.Count; del++) render.MainViewPort.Children.Remove(KMPViewportObject.OBJ_MV3DList[del]);
            KMPViewportObject.OBJ_MV3DList.Clear();

            for (int del = 0; del < KMPViewportObject.Routes_List.Count; del++) HTK_3DES.PathTools.DeleteRail(render, KMPViewportObject.Routes_List[del]);
            KMPViewportObject.Routes_List.Clear();

            for (int del = 0; del < KMPViewportObject.Area_MV3DList.Count; del++) render.MainViewPort.Children.Remove(KMPViewportObject.Area_MV3DList[del]);
            KMPViewportObject.Area_MV3DList.Clear();

            for (int del = 0; del < KMPViewportObject.Camera_MV3DList.Count; del++) render.MainViewPort.Children.Remove(KMPViewportObject.Camera_MV3DList[del]);
            KMPViewportObject.Camera_MV3DList.Clear();

            for (int del = 0; del < KMPViewportObject.RespawnPoint_MV3DList.Count; del++) render.MainViewPort.Children.Remove(KMPViewportObject.RespawnPoint_MV3DList[del]);
            KMPViewportObject.RespawnPoint_MV3DList.Clear();

            for (int del = 0; del < KMPViewportObject.GlideRoute_Rail_List.Count; del++) HTK_3DES.PathTools.DeleteRail(render, KMPViewportObject.GlideRoute_Rail_List[del]);
            KMPViewportObject.GlideRoute_Rail_List.Clear();

            KMP_Group_ListBox.Items.Clear();
            KMP_Path_ListBox.Items.Clear();
            propertyGrid_KMP_Group.SelectedObject = null;
            propertyGrid_KMP_Path.SelectedObject = null;
            propertyGrid_KMP_StageInfo.SelectedObject = null;
            KMPSectionComboBox.Items.Clear();

            writeBinaryToolStripMenuItem.Enabled = false;
            closeKMPToolStripMenuItem.Enabled = false;
            exportToolStripMenuItem.Enabled = false;

            inputXmlAsXXXXToolStripMenuItem.Enabled = false;
            xXXXRouteImporterToolStripMenuItem.Enabled = false;
        }

        private void KMPVisibility_CheckedChanged(object sender, EventArgs e)
        {
            if (CH_Kartpoint.Checked == true) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Area.Checked, render, KMPViewportObject.StartPosition_MV3DList);
            if (CH_Kartpoint.Checked == false) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Area.Checked, render, KMPViewportObject.StartPosition_MV3DList);

            if (CH_EnemyRoutes.Checked == true) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_EnemyRoutes.Checked, render, KMPViewportObject.EnemyRoute_Rail_List);
            if (CH_EnemyRoutes.Checked == false) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_EnemyRoutes.Checked, render, KMPViewportObject.EnemyRoute_Rail_List);

            if (CH_ItemRoutes.Checked == true) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_ItemRoutes.Checked, render, KMPViewportObject.ItemRoute_Rail_List);
            if (CH_ItemRoutes.Checked == false) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_ItemRoutes.Checked, render, KMPViewportObject.ItemRoute_Rail_List);

            if (CH_Checkpoint.Checked == true) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Checkpoint.Checked, render, KMPViewportObject.Checkpoint_Rail);
            if (CH_Checkpoint.Checked == false) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Checkpoint.Checked, render, KMPViewportObject.Checkpoint_Rail);

            if (CH_OBJ.Checked == true) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_OBJ.Checked, render, KMPViewportObject.OBJ_MV3DList);
            if (CH_OBJ.Checked == false) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_OBJ.Checked, render, KMPViewportObject.OBJ_MV3DList);

            if (CH_Routes.Checked == true) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Routes.Checked, render, KMPViewportObject.Routes_List);
            if (CH_Routes.Checked == false) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Routes.Checked, render, KMPViewportObject.Routes_List);

            if (CH_Area.Checked == true) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Area.Checked, render, KMPViewportObject.Area_MV3DList);
            if (CH_Area.Checked == false) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Area.Checked, render, KMPViewportObject.Area_MV3DList);

            if (CH_Camera.Checked == true) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Camera.Checked, render, KMPViewportObject.Camera_MV3DList);
            if (CH_Camera.Checked == false) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Camera.Checked, render, KMPViewportObject.Camera_MV3DList);

            if (CH_Returnpoints.Checked == true) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Returnpoints.Checked, render, KMPViewportObject.RespawnPoint_MV3DList);
            if (CH_Returnpoints.Checked == false) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Returnpoints.Checked, render, KMPViewportObject.RespawnPoint_MV3DList);

            if (CH_GlideRoutes.Checked == true) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_GlideRoutes.Checked, render, KMPViewportObject.GlideRoute_Rail_List);
            if (CH_GlideRoutes.Checked == false) ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_GlideRoutes.Checked, render, KMPViewportObject.GlideRoute_Rail_List);
        }

        private void createKMPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TPTK_Section = new KMPPropertyGridSettings.TPTK_Section
            {
                TPTKValueList = new List<KMPPropertyGridSettings.TPTK_Section.TPTKValue>()
            };

            HPNE_TPNE_Section = new KMPPropertyGridSettings.HPNE_TPNE_Section
            {
                HPNEValueList = new List<KMPPropertyGridSettings.HPNE_TPNE_Section.HPNEValue>()
            };

            HPTI_TPTI_Section = new KMPPropertyGridSettings.HPTI_TPTI_Section
            {
                HPTIValueList = new List<KMPPropertyGridSettings.HPTI_TPTI_Section.HPTIValue>()
            };

            HPKC_TPKC_Section = new KMPPropertyGridSettings.HPKC_TPKC_Section
            {
                HPKCValueList = new List<KMPPropertyGridSettings.HPKC_TPKC_Section.HPKCValue>()
            };

            JBOG_Section = new KMPPropertyGridSettings.JBOG_section
            {
                JBOGValueList = new List<KMPPropertyGridSettings.JBOG_section.JBOGValue>()
            };

            ITOP_Section = new KMPPropertyGridSettings.ITOP_Section
            {
                ITOP_RouteList = new List<KMPPropertyGridSettings.ITOP_Section.ITOP_Route>()
            };

            AERA_Section = new KMPPropertyGridSettings.AERA_Section
            {
                AERAValueList = new List<KMPPropertyGridSettings.AERA_Section.AERAValue>()
            };

            EMAC_Section = new KMPPropertyGridSettings.EMAC_Section
            {
                EMACValueList = new List<KMPPropertyGridSettings.EMAC_Section.EMACValue>()
            };

            TPGJ_Section = new KMPPropertyGridSettings.TPGJ_Section
            {
                TPGJValueList = new List<KMPPropertyGridSettings.TPGJ_Section.TPGJValue>()
            };

            IGTS_Section = new KMPPropertyGridSettings.IGTS_Section
            {
                Unknown1 = 0,
                LapCount = 3,
                PolePosition = 0,
                Unknown2 = 0,
                Unknown3 = 0,
                RGBAColor = new KMPPropertyGridSettings.IGTS_Section.RGBA
                {
                    R = 255,
                    G = 255,
                    B = 255,
                    A = 0
                },
                FlareAlpha = 75
            };

            HPLG_TPLG_Section = new KMPPropertyGridSettings.HPLG_TPLG_Section
            {
                HPLGValueList = new List<KMPPropertyGridSettings.HPLG_TPLG_Section.HPLGValue>()
            };

            if (KMPSectionComboBox.Items.Count == 0)
            {
                string[] AllSectionAry = new string[] { "KartPoint", "EnemyRoutes", "ItemRoutes", "CheckPoint", "Obj", "Route", "Area", "Camera", "JugemPoint", "GlideRoutes" };
                KMPSectionComboBox.Items.AddRange(AllSectionAry.ToArray());
            }

            KMPSectionComboBox.SelectedIndex = 0;

            writeBinaryToolStripMenuItem.Enabled = true;
            closeKMPToolStripMenuItem.Enabled = true;
            exportToolStripMenuItem.Enabled = true;
            inputXmlAsXXXXToolStripMenuItem.Enabled = true;
        }

        private void allSectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog Save_KMPXML = new SaveFileDialog()
            {
                Title = "Save XML(KMP)",
                InitialDirectory = @"C:\Users\User\Desktop",
                Filter = "xml file|*.xml"
                //FileName = ""
            };

            if (Save_KMPXML.ShowDialog() != DialogResult.OK) return;

            KMPPropertyGridSettings kMPPropertyGridSettings = new KMPPropertyGridSettings
            {
                TPTKSection = TPTK_Section,
                HPNE_TPNESection = HPNE_TPNE_Section,
                HPTI_TPTISection = HPTI_TPTI_Section,
                HPKC_TPKCSection = HPKC_TPKC_Section,
                JBOGSection = JBOG_Section,
                ITOPSection = ITOP_Section,
                AERASection = AERA_Section,
                EMACSection = EMAC_Section,
                TPGJSection = TPGJ_Section,
                IGTSSection = IGTS_Section,
                HPLG_TPLGSection = HPLG_TPLG_Section
            };


            XMLExporter.ExportAll(kMPPropertyGridSettings, Save_KMPXML.FileName);
        }

        private void outputXXXXAsXmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog Save_KMPXML = new SaveFileDialog()
            {
                Title = "Save XML(KMP)",
                InitialDirectory = @"C:\Users\User\Desktop",
                Filter = "xml file|*.xml"
                //FileName = ""
            };

            if (Save_KMPXML.ShowDialog() != DialogResult.OK) return;

            KMPPropertyGridSettings kMPPropertyGridSettings = new KMPPropertyGridSettings
            {
                TPTKSection = TPTK_Section,
                HPNE_TPNESection = HPNE_TPNE_Section,
                HPTI_TPTISection = HPTI_TPTI_Section,
                HPKC_TPKCSection = HPKC_TPKC_Section,
                JBOGSection = JBOG_Section,
                ITOPSection = ITOP_Section,
                AERASection = AERA_Section,
                EMACSection = EMAC_Section,
                TPGJSection = TPGJ_Section,
                IGTSSection = IGTS_Section,
                HPLG_TPLGSection = HPLG_TPLG_Section
            };

            if (KMPSectionComboBox.Text == "KartPoint") XMLExporter.ExportSection(kMPPropertyGridSettings, Save_KMPXML.FileName, TestXml.KMPXmlSetting.Section.KartPoint);
            if (KMPSectionComboBox.Text == "EnemyRoutes") XMLExporter.ExportSection(kMPPropertyGridSettings, Save_KMPXML.FileName, TestXml.KMPXmlSetting.Section.EnemyRoutes);
            if (KMPSectionComboBox.Text == "ItemRoutes") XMLExporter.ExportSection(kMPPropertyGridSettings, Save_KMPXML.FileName, TestXml.KMPXmlSetting.Section.ItemRoutes);
            if (KMPSectionComboBox.Text == "CheckPoint") XMLExporter.ExportSection(kMPPropertyGridSettings, Save_KMPXML.FileName, TestXml.KMPXmlSetting.Section.CheckPoint);
            if (KMPSectionComboBox.Text == "Obj") XMLExporter.ExportSection(kMPPropertyGridSettings, Save_KMPXML.FileName, TestXml.KMPXmlSetting.Section.Obj);
            if (KMPSectionComboBox.Text == "Route") XMLExporter.ExportSection(kMPPropertyGridSettings, Save_KMPXML.FileName, TestXml.KMPXmlSetting.Section.Route);
            if (KMPSectionComboBox.Text == "Area") XMLExporter.ExportSection(kMPPropertyGridSettings, Save_KMPXML.FileName, TestXml.KMPXmlSetting.Section.Area);
            if (KMPSectionComboBox.Text == "Camera") XMLExporter.ExportSection(kMPPropertyGridSettings, Save_KMPXML.FileName, TestXml.KMPXmlSetting.Section.Camera);
            if (KMPSectionComboBox.Text == "JugemPoint") XMLExporter.ExportSection(kMPPropertyGridSettings, Save_KMPXML.FileName, TestXml.KMPXmlSetting.Section.JugemPoint);
            if (KMPSectionComboBox.Text == "GlideRoutes") XMLExporter.ExportSection(kMPPropertyGridSettings, Save_KMPXML.FileName, TestXml.KMPXmlSetting.Section.GlideRoutes);
        }

        private void XXXXRouteExporterTSM_Click(object sender, EventArgs e)
        {
            SaveFileDialog Save_KMPXML = new SaveFileDialog()
            {
                Title = "Save XML(KMP)",
                InitialDirectory = @"C:\Users\User\Desktop",
                Filter = "xml file|*.xml"
                //FileName = ""
            };

            if (Save_KMPXML.ShowDialog() != DialogResult.OK) return;

            KMPPropertyGridSettings kMPPropertyGridSettings = new KMPPropertyGridSettings
            {
                TPTKSection = TPTK_Section,
                HPNE_TPNESection = HPNE_TPNE_Section,
                HPTI_TPTISection = HPTI_TPTI_Section,
                HPKC_TPKCSection = HPKC_TPKC_Section,
                JBOGSection = JBOG_Section,
                ITOPSection = ITOP_Section,
                AERASection = AERA_Section,
                EMACSection = EMAC_Section,
                TPGJSection = TPGJ_Section,
                IGTSSection = IGTS_Section,
                HPLG_TPLGSection = HPLG_TPLG_Section
            };

            if (KMPSectionComboBox.Text == "EnemyRoutes") XMLExporter.ExportXXXXRoute(kMPPropertyGridSettings, Save_KMPXML.FileName, TestXml.XXXXRouteXmlSetting.RouteType.EnemyRoute);
            if (KMPSectionComboBox.Text == "ItemRoutes") XMLExporter.ExportXXXXRoute(kMPPropertyGridSettings, Save_KMPXML.FileName, TestXml.XXXXRouteXmlSetting.RouteType.ItemRoute);
            if (KMPSectionComboBox.Text == "GlideRoutes") XMLExporter.ExportXXXXRoute(kMPPropertyGridSettings, Save_KMPXML.FileName, TestXml.XXXXRouteXmlSetting.RouteType.GlideRoute);
        }

        private void ImportAllSectionTSM_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog()
            {
                Title = "Open Xml",
                InitialDirectory = @"C:\Users\User\Desktop",
                Filter = "xml file|*.xml"
            };

            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;

            KMPViewportObject = new KMPs.KMPViewportObject
            {
                Area_MV3DList = new List<ModelVisual3D>(),
                Camera_MV3DList = new List<ModelVisual3D>(),
                EnemyRoute_Rail_List = new List<HTK_3DES.PathTools.Rail>(),
                ItemRoute_Rail_List = new List<HTK_3DES.PathTools.Rail>(),
                GlideRoute_Rail_List = new List<HTK_3DES.PathTools.Rail>(),
                Routes_List = new List<HTK_3DES.PathTools.Rail>(),
                Checkpoint_Rail = new List<HTK_3DES.KMP_3DCheckpointSystem.Checkpoint>(),
                OBJ_MV3DList = new List<ModelVisual3D>(),
                RespawnPoint_MV3DList = new List<ModelVisual3D>(),
                StartPosition_MV3DList = new List<ModelVisual3D>()
            };

            KMPPropertyGridSettings kMPPropertyGridSettings = XMLImporter.ImportAll(KMPViewportObject, openFileDialog1.FileName, render, textBox1.Text);

            TPTK_Section = kMPPropertyGridSettings.TPTKSection;
            HPNE_TPNE_Section = kMPPropertyGridSettings.HPNE_TPNESection;
            HPTI_TPTI_Section = kMPPropertyGridSettings.HPTI_TPTISection;
            HPKC_TPKC_Section = kMPPropertyGridSettings.HPKC_TPKCSection;
            JBOG_Section = kMPPropertyGridSettings.JBOGSection;
            ITOP_Section = kMPPropertyGridSettings.ITOPSection;
            AERA_Section = kMPPropertyGridSettings.AERASection;
            EMAC_Section = kMPPropertyGridSettings.EMACSection;
            TPGJ_Section = kMPPropertyGridSettings.TPGJSection;
            IGTS_Section = kMPPropertyGridSettings.IGTSSection;
            HPLG_TPLG_Section = kMPPropertyGridSettings.HPLG_TPLGSection;

            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Area.Checked, render, KMPViewportObject.StartPosition_MV3DList);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_EnemyRoutes.Checked, render, KMPViewportObject.EnemyRoute_Rail_List);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_ItemRoutes.Checked, render, KMPViewportObject.ItemRoute_Rail_List);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Checkpoint.Checked, render, KMPViewportObject.Checkpoint_Rail);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_OBJ.Checked, render, KMPViewportObject.OBJ_MV3DList);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Routes.Checked, render, KMPViewportObject.Routes_List);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Area.Checked, render, KMPViewportObject.Area_MV3DList);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Camera.Checked, render, KMPViewportObject.Camera_MV3DList);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_Returnpoints.Checked, render, KMPViewportObject.RespawnPoint_MV3DList);
            ViewPortObjVisibleSetting.ViewportObj_Visibility(CH_GlideRoutes.Checked, render, KMPViewportObject.GlideRoute_Rail_List);

            string[] AllSectionAry = new string[] { "KartPoint", "EnemyRoutes", "ItemRoutes", "CheckPoint", "Obj", "Route", "Area", "Camera", "JugemPoint", "GlideRoutes" };
            KMPSectionComboBox.Items.AddRange(AllSectionAry.ToArray());
            KMPSectionComboBox.SelectedIndex = 0;

            if (IGTS_Section != null)
            {
                //Display only IGTS section directly to PropertyGrid
                propertyGrid_KMP_StageInfo.SelectedObject = IGTS_Section;
            }

            writeBinaryToolStripMenuItem.Enabled = true;
            closeKMPToolStripMenuItem.Enabled = true;
            exportToolStripMenuItem.Enabled = true;
            xXXXRouteImporterToolStripMenuItem.Enabled = true;
            inputXmlAsXXXXToolStripMenuItem.Enabled = true;
        }

        private void InputXmlAsXXXXTSM_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog()
            {
                Title = "Open Xml",
                InitialDirectory = @"C:\Users\User\Desktop",
                Filter = "xml file|*.xml"
            };

            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;

            if (KMPSectionComboBox.Text == "KartPoint")
            {
                for (int del = 0; del < KMPViewportObject.StartPosition_MV3DList.Count; del++) render.MainViewPort.Children.Remove(KMPViewportObject.StartPosition_MV3DList[del]);
                KMPViewportObject.StartPosition_MV3DList.Clear();

                KMPViewportObject.StartPosition_MV3DList = new List<ModelVisual3D>();
                TPTK_Section = XMLImporter.ImportKartPosition(KMPViewportObject, openFileDialog1.FileName, render);
            }
            if (KMPSectionComboBox.Text == "EnemyRoutes")
            {
                for (int del = 0; del < KMPViewportObject.EnemyRoute_Rail_List.Count; del++) HTK_3DES.PathTools.DeleteRail(render, KMPViewportObject.EnemyRoute_Rail_List[del]);
                KMPViewportObject.EnemyRoute_Rail_List.Clear();

                KMPViewportObject.EnemyRoute_Rail_List = new List<HTK_3DES.PathTools.Rail>();
                HPNE_TPNE_Section = XMLImporter.ImportEnemyRoute(KMPViewportObject, openFileDialog1.FileName, render);
            }
            if (KMPSectionComboBox.Text == "ItemRoutes")
            {
                for (int del = 0; del < KMPViewportObject.ItemRoute_Rail_List.Count; del++) HTK_3DES.PathTools.DeleteRail(render, KMPViewportObject.ItemRoute_Rail_List[del]);
                KMPViewportObject.ItemRoute_Rail_List.Clear();

                KMPViewportObject.ItemRoute_Rail_List = new List<HTK_3DES.PathTools.Rail>();
                HPTI_TPTI_Section = XMLImporter.ImportItemRoute(KMPViewportObject, openFileDialog1.FileName, render);
            }
            if (KMPSectionComboBox.Text == "CheckPoint")
            {
                for (int del = 0; del < KMPViewportObject.Checkpoint_Rail.Count; del++) HTK_3DES.KMP_3DCheckpointSystem.DeleteRailChk(render, KMPViewportObject.Checkpoint_Rail[del]);
                KMPViewportObject.ItemRoute_Rail_List.Clear();

                KMPViewportObject.Checkpoint_Rail = new List<HTK_3DES.KMP_3DCheckpointSystem.Checkpoint>();
                HPKC_TPKC_Section = XMLImporter.ImportCheckpoint(KMPViewportObject, openFileDialog1.FileName, render, textBox1.Text);
            }
            if (KMPSectionComboBox.Text == "Obj")
            {
                for (int del = 0; del < KMPViewportObject.OBJ_MV3DList.Count; del++) render.MainViewPort.Children.Remove(KMPViewportObject.OBJ_MV3DList[del]);
                KMPViewportObject.OBJ_MV3DList.Clear();

                KMPViewportObject.OBJ_MV3DList = new List<ModelVisual3D>();
                JBOG_Section = XMLImporter.ImportObject(KMPViewportObject, openFileDialog1.FileName, render);
            }
            if (KMPSectionComboBox.Text == "Route")
            {
                for (int del = 0; del < KMPViewportObject.Routes_List.Count; del++) HTK_3DES.PathTools.DeleteRail(render, KMPViewportObject.Routes_List[del]);
                KMPViewportObject.Routes_List.Clear();

                KMPViewportObject.Routes_List = new List<HTK_3DES.PathTools.Rail>();
                ITOP_Section = XMLImporter.ImportRoute(KMPViewportObject, openFileDialog1.FileName, render);
            }
            if (KMPSectionComboBox.Text == "Area")
            {
                for (int del = 0; del < KMPViewportObject.Area_MV3DList.Count; del++) render.MainViewPort.Children.Remove(KMPViewportObject.Area_MV3DList[del]);
                KMPViewportObject.Area_MV3DList.Clear();

                KMPViewportObject.Area_MV3DList = new List<ModelVisual3D>();
                AERA_Section = XMLImporter.ImportArea(KMPViewportObject, openFileDialog1.FileName, render);
            }
            if (KMPSectionComboBox.Text == "Camera")
            {
                for (int del = 0; del < KMPViewportObject.Camera_MV3DList.Count; del++) render.MainViewPort.Children.Remove(KMPViewportObject.Camera_MV3DList[del]);
                KMPViewportObject.Camera_MV3DList.Clear();

                KMPViewportObject.Camera_MV3DList = new List<ModelVisual3D>();
                EMAC_Section = XMLImporter.ImportCamera(KMPViewportObject, openFileDialog1.FileName, render);
            }
            if (KMPSectionComboBox.Text == "JugemPoint")
            {
                for (int del = 0; del < KMPViewportObject.RespawnPoint_MV3DList.Count; del++) render.MainViewPort.Children.Remove(KMPViewportObject.RespawnPoint_MV3DList[del]);
                KMPViewportObject.RespawnPoint_MV3DList.Clear();

                KMPViewportObject.RespawnPoint_MV3DList = new List<ModelVisual3D>();
                TPGJ_Section = XMLImporter.ImportJugemPoint(KMPViewportObject, openFileDialog1.FileName, render);
            }
            if (KMPSectionComboBox.Text == "GlideRoutes")
            {
                for (int del = 0; del < KMPViewportObject.GlideRoute_Rail_List.Count; del++) HTK_3DES.PathTools.DeleteRail(render, KMPViewportObject.GlideRoute_Rail_List[del]);
                KMPViewportObject.GlideRoute_Rail_List.Clear();

                KMPViewportObject.GlideRoute_Rail_List = new List<HTK_3DES.PathTools.Rail>();
                HPLG_TPLG_Section = XMLImporter.ImportGlideRoute(KMPViewportObject, openFileDialog1.FileName, render);
            }

            KMPSectionComboBox.SelectedIndex = 0;
        }

        private void xXXXRouteImporterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog()
            {
                Title = "Open Xml",
                InitialDirectory = @"C:\Users\User\Desktop",
                Filter = "xml file|*.xml"
            };

            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;

            if (KMPSectionComboBox.Text == "EnemyRoutes")
            {
                for (int del = 0; del < KMPViewportObject.EnemyRoute_Rail_List.Count; del++) HTK_3DES.PathTools.DeleteRail(render, KMPViewportObject.EnemyRoute_Rail_List[del]);
                KMPViewportObject.EnemyRoute_Rail_List.Clear();

                KMPViewportObject.EnemyRoute_Rail_List = new List<HTK_3DES.PathTools.Rail>();
                HPNE_TPNE_Section = XMLImporter.ImportEnemyRoutePositionAndScaleOnly(KMPViewportObject, openFileDialog1.FileName, render);
            }
            if (KMPSectionComboBox.Text == "ItemRoutes")
            {
                for (int del = 0; del < KMPViewportObject.ItemRoute_Rail_List.Count; del++) HTK_3DES.PathTools.DeleteRail(render, KMPViewportObject.ItemRoute_Rail_List[del]);
                KMPViewportObject.ItemRoute_Rail_List.Clear();

                KMPViewportObject.ItemRoute_Rail_List = new List<HTK_3DES.PathTools.Rail>();
                HPTI_TPTI_Section = XMLImporter.ImportItemRoutePositionAndScaleOnly(KMPViewportObject, openFileDialog1.FileName, render);
            }
            if (KMPSectionComboBox.Text == "GlideRoutes")
            {
                for (int del = 0; del < KMPViewportObject.GlideRoute_Rail_List.Count; del++) HTK_3DES.PathTools.DeleteRail(render, KMPViewportObject.GlideRoute_Rail_List[del]);
                KMPViewportObject.GlideRoute_Rail_List.Clear();

                KMPViewportObject.GlideRoute_Rail_List = new List<HTK_3DES.PathTools.Rail>();
                HPLG_TPLG_Section = XMLImporter.ImportGlideRoutePositionAndScaleOnly(KMPViewportObject, openFileDialog1.FileName, render);
            }

            KMPSectionComboBox.SelectedIndex = 0;
        }

        private void KMP_Path_ListBox_DoubleClick(object sender, EventArgs e)
        {
            if (KMPSectionComboBox.Text == "KartPoint")
            {
                render.FindObject(TPTK_Section.TPTKValueList, KMP_Path_ListBox.SelectedIndex);
            }
            if (KMPSectionComboBox.Text == "EnemyRoutes")
            {
                if (tabControl1.SelectedIndex == 1) render.FindObject(HPNE_TPNE_Section.HPNEValueList, KMP_Path_ListBox.SelectedIndex, KMP_Group_ListBox.SelectedIndex);
            }
            if (KMPSectionComboBox.Text == "ItemRoutes")
            {
                if (tabControl1.SelectedIndex == 1) render.FindObject(HPTI_TPTI_Section.HPTIValueList, KMP_Path_ListBox.SelectedIndex, KMP_Group_ListBox.SelectedIndex);
            }
            if (KMPSectionComboBox.Text == "CheckPoint")
            {
                if (KMPChkpt_RDTBtn_L.Checked == true)
                {
                    double OffsetValue = Convert.ToDouble(textBox1.Text);
                    render.FindObject(HPKC_TPKC_Section.HPKCValueList, KMP_Path_ListBox.SelectedIndex, KMP_Group_ListBox.SelectedIndex, UserControl1.CheckpointSearchOption.Left, OffsetValue);
                }
                if (KMPChkpt_RDTBtn_R.Checked == true)
                {
                    double OffsetValue = Convert.ToDouble(textBox1.Text);
                    render.FindObject(HPKC_TPKC_Section.HPKCValueList, KMP_Path_ListBox.SelectedIndex, KMP_Group_ListBox.SelectedIndex, UserControl1.CheckpointSearchOption.Right, OffsetValue);
                }
            }
            if (KMPSectionComboBox.Text == "Obj")
            {
                render.FindObject(JBOG_Section.JBOGValueList, KMP_Path_ListBox.SelectedIndex);
            }
            if (KMPSectionComboBox.Text == "Route")
            {
                if (tabControl1.SelectedIndex == 1) render.FindObject(ITOP_Section.ITOP_RouteList, KMP_Path_ListBox.SelectedIndex, KMP_Group_ListBox.SelectedIndex);
            }
            if (KMPSectionComboBox.Text == "Area")
            {
                render.FindObject(AERA_Section.AERAValueList, KMP_Path_ListBox.SelectedIndex);
            }
            if (KMPSectionComboBox.Text == "Camera")
            {
                render.FindObject(EMAC_Section.EMACValueList, KMP_Path_ListBox.SelectedIndex);
            }
            if (KMPSectionComboBox.Text == "JugemPoint")
            {
                render.FindObject(TPGJ_Section.TPGJValueList, KMP_Path_ListBox.SelectedIndex);
            }
            if (KMPSectionComboBox.Text == "GlideRoutes")
            {
                if (tabControl1.SelectedIndex == 1) render.FindObject(HPLG_TPLG_Section.HPLGValueList, KMP_Path_ListBox.SelectedIndex, KMP_Group_ListBox.SelectedIndex);
            }
        }
    }
}

