﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TechObject;
using Device;
using IO;
using System.Globalization;
using System.Drawing;

namespace EasyEPlanner
{
    /// <summary>
    /// Класс, собирающий данные для экспорта в Excel
    /// </summary>
    public static class ExcelDataCollector
    {
        /// <summary>
        /// Сохранить операции и устройства технологических устройств 
        /// в виде дерева.
        /// </summary>
        /// <returns></returns>
        static public TreeView SaveTechObjectOperationsAndActionsAsTree()
        {
            var tree = new TreeView();
            foreach (var techObj in techObjectManager.TechObjects)
            {
                string techName = techObjectManager.GetTechObjectN(techObj)
                    .ToString() + ". " + techObj.EditText[0] + " " + 
                    techObj.TechNumber.ToString();
                var objectNode = new TreeNode(techName);
                objectNode.Tag = techObj;
                
                foreach (var mode in techObj.ModesManager.Modes)
                {
                    string modeName = techObj.ModesManager.GetModeN(mode)
                        .ToString() + ". " + mode.EditText[0];
                    var modeNode = new TreeNode();
                    SaveModesToTreeNodeForExcelExport(mode, modeName, modeNode);
                    objectNode.Nodes.Add(modeNode);
                }
                tree.Nodes.Add(objectNode);
            }
            return tree;
        }

        /// <summary>
        /// Сохранить операции для страницы 
        /// "Технологические операции и устройства"
        /// </summary>
        /// <param name="mode">Операция</param>
        /// <param name="modeName">Имя операции</param>
        /// <param name="modeNode">Узел дерева для сохранения</param>
        private static void SaveModesToTreeNodeForExcelExport(Mode mode,
            string modeName, TreeNode modeNode)
        {
            var res = new string[]
            {
                modeName,
                "", "", "", "", "", "", "", "", "", ""
            };
            modeNode.Tag = res;

            foreach (var state in mode.States)
            {
                FillState(state, ref modeNode);
                FillSteps(state, ref modeNode);
            }
        }

        /// <summary>
        /// Заполнение информации о состоянии операции
        /// (шаг - Во время операции).
        /// </summary>
        /// <param name="state">Состояние операции</param>
        /// <param name="modeNode">узел дерева операции</param>
        private static void FillState(State state, ref TreeNode modeNode)
        {
            Step commonStep;
            if (state.Empty == false)
            {
                commonStep = state.Steps.First();
                var stateNode = new TreeNode();

                const int OpenedDevices = 1;
                const int ClosedDevices = 3;
                const int UpperSeats = 4;
                const int LowerSeats = 5;
                const int RequiredFB = 6;
                const int WashDevices = 7;
                const int DIDOGroup = 8;
                const int AIAOGroup = 9;

                const int DevicesDI = 0;
                const int DevicesDO = 1;
                const int Devices = 2;

                var devicesAction = commonStep.GetActions[WashDevices];
                
                var openedDevices = string.Join(spaceStr, commonStep
                    .GetActions[OpenedDevices].DevicesNames);
                var closedDevices = string.Join(spaceStr, commonStep
                    .GetActions[ClosedDevices].DevicesNames);
                var upperSeats = string.Join(spaceStr, commonStep
                    .GetActions[UpperSeats].DevicesNames);
                var lowerSeats = string.Join(spaceStr, commonStep
                    .GetActions[LowerSeats].DevicesNames);
                var requiredFB = string.Join(spaceStr, commonStep
                    .GetActions[RequiredFB].DevicesNames);
                var dIDOGroup = string.Join(spaceStr, commonStep
                    .GetActions[DIDOGroup].DevicesNames);
                var aIAOGroup = string.Join(spaceStr, commonStep
                    .GetActions[AIAOGroup].DevicesNames);

                var rowWithState = new string[]
                {
                    state.DisplayText.First(),
                    openedDevices,
                    closedDevices,
                    upperSeats,
                    lowerSeats,
                    requiredFB,
                    GenerateGroupWashActionText(devicesAction, DevicesDI),
                    GenerateGroupWashActionText(devicesAction, DevicesDO),
                    GenerateGroupWashActionText(devicesAction, Devices),
                    dIDOGroup,
                    aIAOGroup,
                };
                stateNode.Tag = rowWithState;
                modeNode.Nodes.Add(stateNode);
            }
        }

        /// <summary>
        /// Генерация описания групп действия "Устройства" (старая мойка).
        /// </summary>
        /// <param name="devicesAction">Действие</param>
        /// <param name="subGroupNum">Номер подгруппы действия внутри группы
        /// </param>
        /// <returns></returns>
        private static string GenerateGroupWashActionText(IAction devicesAction,
            int subGroupNum)
        {
            string res = "";

            for (int i = 0; i < devicesAction.SubActions.Count; i++)
            {
                int groupNum = i + 1;
                var group = devicesAction.SubActions[i];

                string groupText = string.Join(" ",
                    group.SubActions[subGroupNum].DevicesNames);
                bool notEmptyGroup = groupText != string.Empty;
                if (notEmptyGroup)
                {
                    res += $"Группа {groupNum}: {groupText}.\n";
                }
            }

            return res;
        }

        /// <summary>
        /// Заполнение шагов операции (кроме шага "Во время операции").
        /// </summary>
        /// <param name="state">Состояние операции</param>
        /// <param name="modeNode">Узел дерева операции</param>
        private static void FillSteps(State state, ref TreeNode modeNode)
        {
            for (int i = 1; i < state.Steps.Count; i++)
            {
                var stepNode = new TreeNode();
                Step commonStep = state.Steps[i];
                string stepName;

                const int OpenedDevices = 0;
                const int ClosedDevices = 2;
                const int UpperSeats = 3;
                const int LowerSeats = 4;

                stepName = i.ToString() + ". " + commonStep.EditText.First();
                var openedDevices = string.Join(spaceStr, commonStep
                    .GetActions[OpenedDevices].DevicesNames);
                var closedDevices = string.Join(spaceStr, commonStep
                    .GetActions[ClosedDevices].DevicesNames);
                var upperSeats = string.Join(spaceStr, commonStep
                    .GetActions[UpperSeats].DevicesNames);
                var lowerSeats = string.Join(spaceStr, commonStep
                    .GetActions[LowerSeats].DevicesNames);
                var resStep = new string[]
                {
                    stepName,
                    openedDevices,
                    closedDevices,
                    upperSeats,
                    lowerSeats,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty
                };
                stepNode.Tag = resStep;
                modeNode.Nodes.Add(stepNode);
            }
        }

        /// <summary>
        /// Сохранить параметры технологических объектов в виде дерева.
        /// </summary>
        /// <returns></returns>
        static public TreeView SaveParamsAsTree()
        {
            var tree = new TreeView();
            foreach (var techObj in techObjectManager.TechObjects)
            {
                string techName = techObjectManager.GetTechObjectN(techObj)
                    .ToString() + ". " + techObj.EditText[0] + " " +
                    techObj.TechNumber.ToString();
                var objectNode = new TreeNode(techName);
                objectNode.Tag = techObj;

                foreach(Params paramsGroup in techObj.GetParamsManager().Items)
                {
                    var parTypeNode = new TreeNode(paramsGroup
                        .NameForChannelBase);
                    parTypeNode.Tag = paramsGroup.NameForChannelBase;
                    objectNode.Nodes.Add(parTypeNode);

                    int counter = 1;
                    foreach(Param param in paramsGroup.Items)
                    {
                        string parName = $"{counter}. {param.EditText[0]}";
                        var parNode = new TreeNode(parName);
                        counter++;
                        parNode.Tag = new string[]
                        {
                            parName,
                            param.GetValue(),
                            param.GetMeter(),
                            param.Operations,
                            param.GetNameLua()
                        };

                        parTypeNode.Nodes.Add(parNode);
                    }
                }

                tree.Nodes.Add(objectNode);
            }
            return tree;
        }

        /// <summary>
        /// Сохранить технологические объекты без действий в виде дерева.
        /// </summary>
        /// <returns></returns>
        static public TreeView SaveObjectsWithoutActionsAsTree()
        {
            var tree = new TreeView();

            bool objectTitleIsWrited = false;
            foreach (var techObj in techObjectManager.TechObjects)
            {
                if (objectTitleIsWrited == false)
                {
                    var titleNode = new TreeNode();
                    titleNode.Tag = new string[] 
                    { 
                        "Технологический объект",
                        "Номер", 
                        "ОУ", 
                        "Имя объекта Monitor" 
                    };
                    tree.Nodes.Add(titleNode);
                    objectTitleIsWrited = true;
                }

                string techName = techObjectManager.GetTechObjectN(techObj)
                    .ToString() + ". " + techObj.EditText[0] + " " + 
                    techObj.TechNumber.ToString();
                var objNode = new TreeNode(techName);
                objNode.Tag = new string[] 
                { 
                    techName, 
                    techObj.TechNumber.ToString(), 
                    techObj.NameEplan, 
                    techObj.NameBC 
                };

                var modesNodes = new TreeNode("Операции");
                WriteObjectsOperationsInNode(ref modesNodes, techObj);

                var parametersNodes = new TreeNode("Параметры");
                WriteObjectParametersInNode(ref parametersNodes, techObj);

                if (modesNodes.Nodes.Count > 0)
                {
                    objNode.Nodes.Add(modesNodes);
                }

                if (parametersNodes.Nodes.Count > 0)
                {
                    objNode.Nodes.Add(parametersNodes);
                }

                tree.Nodes.Add(objNode);
            }
            return tree;
        }

        /// <summary>
        /// Записать операции объекта в узел дерева.
        /// </summary>
        /// <param name="modesNodes">Узел с операциями объекта</param>
        /// <param name="techObject">Объект</param>
        private static void WriteObjectsOperationsInNode(
            ref TreeNode modesNodes, TechObject.TechObject techObject)
        {
            foreach (var mode in techObject.ModesManager.Modes)
            {
                var modeNode = new TreeNode(mode.DisplayText[0]);
                modeNode.Tag = mode;
                WriteObjectOperationStepsInNode(ref modeNode, mode);

                var operationParameterNodes = new TreeNode("Параметры");
                WriteObjectOperationParametersInNode(
                    ref operationParameterNodes, mode);

                if (operationParameterNodes.Nodes.Count > 1)
                {
                    modeNode.Nodes.Add(operationParameterNodes);
                }

                modesNodes.Nodes.Add(modeNode);
            }
        }

        /// <summary>
        /// Записать шаги операции объекта
        /// </summary>
        /// <param name="modeNode">Узел операции</param>
        /// <param name="mode">Операция</param>
        private static void WriteObjectOperationStepsInNode(
            ref TreeNode modeNode, Mode mode)
        {
            foreach (var state in mode.States)
            {
                var stateNodes = new TreeNode(state.DisplayText[0]);
                stateNodes.Tag = state;

                if (state.Steps.Count > 1)
                {
                    bool stepsTitleIsWrited = false;
                    if (stepsTitleIsWrited == false)
                    {
                        stepsTitleIsWrited = true;
                        stateNodes.Nodes.Add("Шаги");
                    }
                    foreach (var step in state.Steps)
                    {
                        if (!step.DisplayText.Contains(Step.MainStepName))
                        {
                            stateNodes.Nodes.Add(step.DisplayText[0]);
                        }
                    }

                    modeNode.Nodes.Add(stateNodes);
                }
            }
        }

        /// <summary>
        /// Записать параметры операции в узел дерева.
        /// </summary>
        /// <param name="operationParameterNodes">Узел параметров операции
        /// </param>
        /// <param name="mode">Операции</param>
        private static void WriteObjectOperationParametersInNode(
            ref TreeNode operationParameterNodes, Mode mode)
        {
            bool operationParameterTitleIsWrited = false;
            for (int i = 0; i < mode.GetOperationParams().Items.Length; i++)
            {
                if (operationParameterTitleIsWrited == false)
                {
                    var titleNode = new TreeNode();
                    titleNode.Tag = new string[]
                    {
                        "Описание параметра",
                        "Lua имя"
                    };
                    operationParameterNodes.Nodes.Add(titleNode);
                    operationParameterTitleIsWrited = true;
                }

                string name = mode.GetOperationParams().Items[i].DisplayText[0];
                OperationParam operationParameter = mode
                    .GetOperationParams().Items[i] as OperationParam;
                var operationParameterNode = new TreeNode(name);
                operationParameterNode.Tag = new string[]
                {
                    name,
                    operationParameter.Param.GetNameLua()
                };
                operationParameterNodes.Nodes.Add(operationParameterNode);
            }
        }

        /// <summary>
        /// Записать параметры технологического объекта в узел дерева
        /// </summary>
        /// <param name="parameters">Узел дерева параметров</param>
        /// <param name="techObject">Технологический объект</param>
        private static void WriteObjectParametersInNode(ref TreeNode parameters,
            TechObject.TechObject techObject)
        {
            bool parameterTitleisWrited = false;
            var floatParameters = techObject.GetParamsManager().Float.Items;
            for (int i = 0; i < floatParameters.Length; i++)
            {
                var parameter = floatParameters[i] as Param;
                if (parameterTitleisWrited == false)
                {
                    var titleNode = new TreeNode();
                    titleNode.Tag = new string[]
                    {
                            "Имя параметра",
                            "Значение",
                            "Размерность",
                            "Операция" ,
                            "Lua имя"
                    };
                    parameters.Nodes.Add(titleNode);
                    parameterTitleisWrited = true;
                }

                string parameterName = (i + 1).ToString() + ". " + 
                    parameter.EditText[0];
                var parameterNode = new TreeNode();
                parameterNode.Tag = new string[] 
                { 
                    parameterName,
                    parameter.GetValue(), 
                    parameter.GetMeter(),
                    parameter.GetOperationN(),
                    parameter.GetNameLua() 
                };
                parameters.Nodes.Add(parameterNode);
            }
        }

        /// <summary>
        /// Сохранить подключение устройств (привязку) в виде дерева.
        /// </summary>
        /// <returns></returns>
        public static TreeView SaveDeviceConnectionAsTree()
        {
            var tree = new TreeView();
            foreach (DeviceType devType in Enum.GetValues(typeof(DeviceType)))
            {
                var typeNode = new TreeNode(devType.ToString());
                typeNode.Tag = devType;
                tree.Nodes.Add(typeNode);
            }

            foreach (var dev in deviceManager.Devices)
            {
                TreeNode parent = null;
                TreeNode devNode = null;

                foreach (TreeNode node in tree.Nodes)
                {
                    if ((DeviceType)node.Tag == dev.DeviceType)
                    {
                        parent = node;
                        break;
                    }
                }

                //Не найден тип устройства.
                if (parent == null)
                {
                    break;
                }

                if (dev.ObjectName != "")
                {
                    string objectName = dev.ObjectName + dev.ObjectNumber;
                    TreeNode devParent = null;

                    foreach (TreeNode node in parent.Nodes)
                    {
                        if ((node.Tag is String) &&
                            (string)node.Tag == objectName)
                        {
                            devParent = node;
                            break;
                        }
                    }

                    if (devParent == null)
                    {
                        devParent = new TreeNode(objectName);
                        devParent.Tag = objectName;
                        parent.Nodes.Add(devParent);
                    }

                    devNode = new TreeNode(dev.Name + " - " + dev.Description);
                    devNode.Tag = dev;
                    devParent.Nodes.Add(devNode);

                }
                else
                {

                    devNode = new TreeNode(dev.Name + " - " + dev.Description);
                    devNode.Tag = dev;
                    parent.Nodes.Add(devNode);
                }

                foreach (var ch in dev.Channels)
                {
                    string chNodeName = "";
                    if (!ch.IsEmpty())
                    {
                        chNodeName = ch.Name + " " + ch.Comment +
                            $" (A{ch.FullModule}:" + ch.PhysicalClamp + ")";
                    }
                    else
                    {
                        chNodeName = ch.Name + " " + ch.Comment;
                    }
                    devNode.Nodes.Add(chNodeName);
                }

            }

            return tree;
        }

        /// <summary>
        /// Сохранение в виде таблицы (массива строк, чисел и т.д.) информации
        /// об устройствах.
        /// </summary>
        public static object[,] SaveDevicesInformationAsArray()
        {
            const int MAX_SUBTYPE_CNT = 20;
            const int MAX_COL = 20;

            int MAX_TYPE_CNT = Enum.GetValues(typeof(DeviceType)).Length;
            var countDev = new int[MAX_TYPE_CNT];
            var countSubDev = new int[MAX_TYPE_CNT, MAX_SUBTYPE_CNT];

            int RowsCnt = deviceManager.Devices.Count;
            var res = new object[RowsCnt, MAX_COL];

            int idx = 0;
            foreach (var dev in deviceManager.Devices)
            {
                idx += dev.SaveAsArray(ref res, idx, MAX_COL);
            }

            return res;
        }

        /// <summary>
        /// Сохранение сводной информации об устройствах в виде таблицы.
        /// </summary>
        public static object[,] SaveDevicesSummaryAsArray()
        {
            const int MAX_ROW = 200;
            const int MAX_COL = 20;
            var res = new object[MAX_ROW, MAX_COL];

            var devices = new Dictionary<string, int>();
            foreach (IODevice dev in deviceManager.Devices)
            {
                
                if (dev.DeviceType == DeviceType.V ||
                    dev.DeviceType == DeviceType.M ||
                    dev.DeviceType == DeviceType.LS)
                {
                    string deviceSubType = dev.GetDeviceSubTypeStr(
                        dev.DeviceType, dev.DeviceSubType);
                    if (devices.ContainsKey(deviceSubType) == false)
                    {
                        devices.Add(deviceSubType, 1);
                    }
                    else
                    {
                        devices[deviceSubType]++;
                    }
                }
                else
                {
                    string deviceType = dev.DeviceType.ToString();
                    if (devices.ContainsKey(dev.DeviceType.ToString()) == false)
                    {
                        devices.Add(deviceType, 1);
                    }
                    else
                    {
                        devices[deviceType]++;
                    }
                }
            }

            // Сводная таблица
            int idx = 0;
            foreach(var devType in devices)
            {
                res[idx, 0] = devType.Key;
                res[idx, 1] = devType.Value;
                idx++;
            }
            res[idx, 0] = "Всего";
            res[idx, 1] = deviceManager.Devices.Count;

            return res;
        }

        /// <summary>
        /// Сохранение подключение устройств к устройствам ввода-вывода.
        /// </summary>
        /// <param name="prjName">Имя проекта</param>
        /// <param name="modulesCount">Количество модулей</param>
        /// <param name="modulesColor">Цвет</param>
        /// <param name="asInterfaceConnection">AS-интерфейс подключения</param>
        /// <returns></returns>
        public static object[,] SaveIOAsConnectionArray(string prjName, 
            Dictionary<string, int> modulesCount, 
            Dictionary<string, Color> modulesColor, 
            Dictionary<string, object[,]> asInterfaceConnection)
        {
            const int MAX_COL = 6;
            int MAX_ROW = ioManager.IONodes.Count;

            foreach (var ioNode in ioManager.IONodes)
            {
                MAX_ROW += ioNode.IOModules.Count;
            }

            MAX_ROW *= 16;
            var res = new object[MAX_ROW, MAX_COL];
            
            int idx = 0;
            for (int i = 0; i < ioManager.IONodes.Count; i++)
            {
                IONode currentNode = ioManager.IONodes[i];
                res[idx, 3] = prjName;
                idx++;
                res[idx, 3] = 
                    $"'{DateTime.Now.ToString(new CultureInfo("RU-ru"))}";

                string nodeName = "";
                if (currentNode.FullN != 1)
                {
                    nodeName = "Узел №" + (currentNode.N - 1) + " Адрес: " +
                        ioManager.IONodes[i].IP;
                }
                else
                {
                    nodeName = "Контроллер. " + " Адрес: " +
                        ioManager.IONodes[i].IP;
                }

                res[idx, 4] = "Вход, бит";
                res[idx, 5] = "Выход, бит";
                res[idx, 0] = nodeName;
                idx++;

                res[idx, 0] = 0;
                nodeName = currentNode.TypeStr.Replace("750-", "");
                res[idx, 1] = nodeName;

                if (!modulesColor.ContainsKey(nodeName))
                {
                    modulesColor.Add(nodeName, Color.Gray);
                }
                idx++;
                currentNode.SaveAsConnectionArray(ref res, ref idx, 
                    modulesCount, modulesColor, currentNode.N - 1, asInterfaceConnection);
            }

            res = DeleteNullObjects(res); 

            return res;
        }

        /// <summary>
        /// Сохранение устройств, в которых контролируются изделия
        /// </summary>
        /// <returns></returns>
        public static object[,] SaveDevicesArticlesInfoAsArray()
        {
            const int MaxLength = 1000;
            const int MaxWidth = 3;
            object[,] devicesWithArticles = new object[MaxLength, MaxWidth];

            var ignoringDevicesTypes = new List<string>()
            {
                "AI",
                "AO",
                "DI",
                "DO",
            };

            var ignoringDevicesSubTypes = new List<string>()
            {
                "M",
                "M_FREQ",
                "M_REV",
                "M_REV_FREQ",
                "M_REV_2",
                "M_REV_FREQ_2",
                "M_REV_2_ERROR",
                "M_REV_FREQ_2_ERROR",
                "FQT_VIRT",
                "LS_VIRT",
                "LT_VIRT",
                "VC",
            };

            var devices = new List<IODevice>();
            foreach (var device in deviceManager.Devices)
            {
                string devType = device.DeviceType.ToString();
                string devSubType = device.GetDeviceSubTypeStr(
                    device.DeviceType, device.DeviceSubType);
                bool ignoreDevice = ignoringDevicesTypes.Contains(devType) ||
                    ignoringDevicesSubTypes.Contains(devSubType);
                if (!ignoreDevice)
                {
                    devices.Add(device);
                }
            }

            devices.Sort((x, y) =>
            {
                int res = 0;
                res = Device.Device.Compare(x, y);
                return res;
            });

            devicesWithArticles[0, 0] = "ОУ устройства";
            devicesWithArticles[0, 1] = "Подтип устройства";
            devicesWithArticles[0, 2] = "Изделие";

            int counter = 1;
            foreach(var device in devices)
            {         
                devicesWithArticles[counter, 0] = device.EplanName;
                devicesWithArticles[counter, 1] = device.GetDeviceSubTypeStr(
                    device.DeviceType, device.DeviceSubType);
                devicesWithArticles[counter, 2] = device.ArticleName;
                counter++;
            }

            devicesWithArticles = DeleteNullObjects(devicesWithArticles);
            return devicesWithArticles;
        }

        /// <summary>
        /// Удалить пустые объекты в конце массива
        /// </summary>
        /// <param name="res">Массив</param>
        /// <returns></returns>
        private static object[,] DeleteNullObjects(object[,] res)
        {
            int firstDimensionLength = res.GetLength(0);
            int secondDimensionLength = res.GetLength(1);
            int countOfFilledElements = 0;
            for(int i = firstDimensionLength - 1; i >= 0; i--)
            {
                for(int j = 0; j < secondDimensionLength; j++)
                {
                    if (res[i,j] != null)
                    {
                        countOfFilledElements++;
                        break;
                    }
                }
            }

            var newResult = new object[countOfFilledElements, 
                secondDimensionLength];
            for(int i = 0; i < newResult.GetLength(0); i++)
            {
                for(int j = 0; j < newResult.GetLength(1); j++)
                {
                    newResult[i, j] = res[i, j];
                }
            }

            return newResult;

        }

        const string spaceStr = " ";

        /// <summary>
        /// Менеджер устройств
        /// </summary>
        static DeviceManager deviceManager = DeviceManager.GetInstance();

        /// <summary>
        /// Менеджер технологических объектов.
        /// </summary>
        static ITechObjectManager techObjectManager = TechObjectManager
            .GetInstance();

        /// <summary>
        /// Менеджер устройств ввода-вывода.
        /// </summary>
        static IOManager ioManager = IOManager.GetInstance();
    }
}
