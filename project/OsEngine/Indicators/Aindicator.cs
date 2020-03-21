﻿/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OsEngine.Entity;

namespace OsEngine.Indicators
{
    public abstract class Aindicator : IIndicator
    {
        public void Init(string name)
        {
            Name = name;
            CanDelete = true;
            Load();

            OnStateChange(IndicatorState.Configure);
        }

        public abstract void OnStateChange(IndicatorState state);

        public abstract void OnProcess(List<Candle> candles, int index);

        #region параметры

        // working with strategy parameters / работа с параметрами стратегии

        /// <summary>
        /// create a Decimal type parameter / 
        /// создать параметр типа Decimal
        /// </summary>
        /// <param name="name">param name / Имя параметра</param>
        /// <param name="value">default value / Значение по умолчанию</param>
        public IndicatorParameterDecimal CreateParameterDecimal(string name, decimal value)
        {
            IndicatorParameter newParameter = _parameters.Find(p => p.Name == name);

            if (newParameter != null)
            {
                return (IndicatorParameterDecimal)newParameter;
            }

            newParameter = new IndicatorParameterDecimal(name, value);

            return (IndicatorParameterDecimal)LoadParameterValues(newParameter);
        }

        /// <summary>
        /// create int parameter / 
        /// создать параметр типа Int
        /// </summary>
        /// <param name="name">param name / Имя параметра</param>
        /// <param name="value">default value / Значение по умолчанию</param>
        public IndicatorParameterInt CreateParameterInt(string name, int value)
        {
            IndicatorParameter newParameter = _parameters.Find(p => p.Name == name);

            if (newParameter != null)
            {
                return (IndicatorParameterInt)newParameter;
            }

            newParameter = new IndicatorParameterInt(name, value);

            return (IndicatorParameterInt)LoadParameterValues(newParameter);
        }

        /// <summary>
        /// create string parameter / 
        /// создать параметр типа String
        /// </summary>
        /// <param name="name">param name / Имя параметра</param>
        /// <param name="value">default value / Значение по умолчанию</param>
        /// <param name="collection">values / Возможные значения для параметра</param>
        public IndicatorParameterString CreateParameterStringCollection(string name, string value, List<string> collection)
        {
            IndicatorParameter newParameter = _parameters.Find(p => p.Name == name);

            if (newParameter != null)
            {
                return (IndicatorParameterString)newParameter;
            }

            newParameter = new IndicatorParameterString(name, value, collection);

            return (IndicatorParameterString)LoadParameterValues(newParameter);
        }

        /// <summary>
        /// create bool type parameter / 
        /// создать параметр типа Bool
        /// </summary>
        /// <param name="name">param name / Имя параметра</param>
        /// <param name="value">default value / Значение по умолчанию</param>
        public IndicatorParameterBool CreateParameterBool(string name, bool value)
        {
            IndicatorParameter newParameter = _parameters.Find(p => p.Name == name);

            if (newParameter != null)
            {
                return (IndicatorParameterBool)newParameter;
            }

            newParameter = new IndicatorParameterBool(name, value);
            return (IndicatorParameterBool)LoadParameterValues(newParameter);
        }

        /// <summary>
        /// load parameter settings / 
        /// загрузить настройки параметра
        /// </summary>
        /// <param name="newParameter">setting parameter you want to load / параметр настройки которого нужно загрузить</param>
        private IndicatorParameter LoadParameterValues(IndicatorParameter newParameter)
        {
            GetValueParameterSaveByUser(newParameter);

            newParameter.ValueChange += Parameter_ValueChange;

            _parameters.Add(newParameter);

            return newParameter;
        }

        /// <summary>
        /// load parameter settings from file / 
        /// загрузить настройки параметра из файла
        /// </summary>
        private void GetValueParameterSaveByUser(IndicatorParameter parameter)
        {
            if (Name == "")
            {
                return;
            }

            if (!File.Exists(@"Engine\" + Name + @"Parametrs.txt"))
            {
                return;
            }
            try
            {
                using (StreamReader reader = new StreamReader(@"Engine\" + Name + @"Parametrs.txt"))
                {
                    while (!reader.EndOfStream)
                    {
                        string[] save = reader.ReadLine().Split('#');

                        if (save[0] == parameter.Name)
                        {
                            parameter.LoadParamFromString(save);
                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        /// <summary>
        /// the list of options available in the panel / 
        /// список параметров доступных у панели
        /// </summary>
        public List<IndicatorParameter> Parameters
        {
            get { return _parameters; }
        }
        private List<IndicatorParameter> _parameters = new List<IndicatorParameter>();

        /// <summary>
        /// parameter has changed settings / 
        /// у параметра изменились настройки
        /// </summary>
        void Parameter_ValueChange()
        {
            if (ParametrsChangeByUser != null)
            {
                ParametrsChangeByUser();
            }
        }

        /// <summary>
        /// save parameter values / 
        /// сохранить значения параметров
        /// </summary>
        private void SaveParametrs()
        {
            if (Name == "")
            {
                return;
            }
            if (_parameters == null ||
                _parameters.Count == 0)
            {
                return;
            }
            try
            {
                using (StreamWriter writer = new StreamWriter(@"Engine\" + Name + @"Parametrs.txt", false)
                    )
                {
                    for (int i = 0; i < _parameters.Count; i++)
                    {
                        writer.WriteLine(_parameters[i].GetStringToSave());
                    }

                    writer.Close();
                }
            }
            catch (Exception)
            {
                // ignore
            }


        }

        /// <summary>
        /// parameter has changed state / 
        /// у параметра изменилось состояние
        /// </summary>
        public event Action ParametrsChangeByUser;

        #endregion

        public void Delete()
        {
            if (File.Exists(@"Engine\" + Name + @"Values.txt"))
            {
                File.Delete(@"Engine\" + Name + @"Values.txt");
            }
            if (File.Exists(@"Engine\" + Name + @"Parametrs.txt"))
            {
                File.Delete(@"Engine\" + Name + @"Parametrs.txt");
            }

            for (int i = 0; IncludeIndicators != null && i < IncludeIndicators.Count; i++)
            {
                IncludeIndicators[i].Delete();
            }

            for (int i = 0; DataSeries != null && i < DataSeries.Count; i++)
            {
                DataSeries[i].Dispose();
            }

            OnStateChange(IndicatorState.Dispose);
        }

        public void Load()
        {

        }

        public void Save()
        {
            if (Name == "")
            {
                return;
            }

            SaveParametrs();
            SaveSeries();
        }

        public void ShowDialog()
        {
            AIndicatorUi ui = new AIndicatorUi(this);
            ui.ShowDialog();

            if (ui.IsAccepted)
            {
                Reload();

                Save();
            }
        }

        #region встроенные индикаторы для прогрузки свечками

        public List<IIndicator> IncludeIndicators = new List<IIndicator>();

        public List<string> IncludeIndicatorsName = new List<string>();

        public void ProcessIndicator(string indicatorName, IIndicator indicator)
        {
            IncludeIndicators.Add(indicator);
            IncludeIndicatorsName.Add(indicatorName);
        }

        #endregion

        #region серии данных

        public IndicatorDataSeries CreateSeries(string name, Color color,
            IndicatorChartPaintType chartPaintType, bool isPaint)
        {
            if (DataSeries.Find(val => val.Name == name) != null)
            {
                return DataSeries.Find(val => val.Name == name);
            }

            IndicatorDataSeries newSeries = new IndicatorDataSeries(color, name, chartPaintType, isPaint);
            DataSeries.Add(newSeries);
            CheckSeriesParamsInSaveData(newSeries);

            return newSeries;
        }

        public List<IndicatorDataSeries> DataSeries = new List<IndicatorDataSeries>();

        /// <summary>
        /// попробовать загрузить ранее сохранённую сервию
        /// </summary>
        /// <param name="series"></param>
        private void CheckSeriesParamsInSaveData(IndicatorDataSeries series)
        {
            if (Name == "")
            {
                return;
            }

            if (!File.Exists(@"Engine\" + Name + @"Values.txt"))
            {
                return;
            }
            try
            {
                using (StreamReader reader = new StreamReader(@"Engine\" + Name + @"Values.txt"))
                {
                    while (!reader.EndOfStream)
                    {
                        string[] save = reader.ReadLine().Split('&');

                        if (save[0] == series.Name)
                        {
                            series.LoadFromStr(save);
                        }
                    }
                    reader.Close();
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public void SaveSeries()
        {
            if (Name == "")
            {
                return;
            }
            if (DataSeries == null ||
                DataSeries.Count == 0)
            {
                return;
            }
            try
            {
                using (StreamWriter writer = new StreamWriter(@"Engine\" + Name + @"Values.txt", false)
                )
                {
                    for (int i = 0; i < DataSeries.Count; i++)
                    {
                        writer.WriteLine(DataSeries[i].GetSaveStr());
                    }

                    writer.Close();
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        /// <summary>
        /// all indicator values/все значения индикатора
        /// </summary>
        List<List<decimal>> IIndicator.ValuesToChart
        {
            get { return null; }
        }

        /// <summary>
        /// indicator colors/цвета для индикатора
        /// </summary>
        List<Color> IIndicator.Colors
        {
            get { return null; }
        }

        #endregion

        #region сервисная информация

        public IndicatorChartPaintType TypeIndicator { get; set; }

        public bool CanDelete { get; set; }

        public string NameSeries { get; set; }

        public string NameArea { get; set; }

        public string Name { get; set; }

        public bool PaintOn { get; set; }

        #endregion

        /// <summary>
        /// reload indicator
        /// перезагрузить индикатор
        /// </summary>
        public void Reload()
        {
            if (_myCandles == null)
            {
                return;
            }

            ProcessAll(_myCandles);

            if (NeadToReloadEvent != null)
            {
                NeadToReloadEvent(this);
            }
        }

        public event Action<IIndicator> NeadToReloadEvent;

        public void Clear()
        {
            _myCandles = new List<Candle>();

            for (int i = 0; i < DataSeries.Count; i++)
            {
                DataSeries[i].Values.Clear();
            }

        }

        private List<Candle> _myCandles = new List<Candle>();

        public void Process(List<Candle> candles)
        {
            if (candles.Count == 0)
            {
                return;
            }
            if (_myCandles == null ||
                candles.Count < _myCandles.Count ||
                candles.Count > _myCandles.Count + 1)
            {
                ProcessAll(candles);
            }
            else if (_myCandles.Count == candles.Count)
            {
                ProcessLast(candles);
            }
            else if (_myCandles.Count + 1 == candles.Count)
            {
                ProcessNew(candles, candles.Count);
            }

            _myCandles = candles;
        }

        private void ProcessAll(List<Candle> candles)
        {
            for (int i = 0; i < IncludeIndicators.Count; i++)
            {
                IncludeIndicators[i].Clear();
                IncludeIndicators[i].Process(candles);
            }

            for (int i = 0; i < DataSeries.Count; i++)
            {
                DataSeries[i].Values.Clear();
            }

            for (int i = 0; i < candles.Count; i++)
            {
                ProcessNew(candles, i);
            }
        }

        private void ProcessLast(List<Candle> candles)
        {
            for (int i = 0; i < IncludeIndicators.Count; i++)
            {
                IncludeIndicators[i].Process(candles);
            }

            for (int i = 0; i < DataSeries.Count; i++)
            {
                while (DataSeries[i].Values.Count < candles.Count)
                {
                    DataSeries[i].Values.Add(0);
                }
            }
            OnProcess(candles, candles.Count - 1);
        }

        private void ProcessNew(List<Candle> candles, int index)
        {
            for (int i = 0; i < IncludeIndicators.Count; i++)
            {
                IncludeIndicators[i].Process(candles);
            }
            for (int i = 0; i < DataSeries.Count; i++)
            {
                while (DataSeries[i].Values.Count < index + 1)
                {
                    DataSeries[i].Values.Add(0);
                }
            }

            OnProcess(candles, index);
        }

    }

    public class IndicatorDataSeries
    {
        public IndicatorDataSeries(Color color, string name, IndicatorChartPaintType paintType, bool isPaint)
        {
            Name = name;
            Color = color;
            ChartPaintType = paintType;
            IsPaint = isPaint;
        }

        public string Name;

        public Color Color;

        public IndicatorChartPaintType ChartPaintType;

        public bool IsPaint;

        public string NameSeries;

        public List<decimal> Values = new List<decimal>();

        public string GetSaveStr()
        {
            string result = "";

            result += Name + "&";

            result += Color.ToArgb() + "&";

            result += ChartPaintType + "&";

            result += IsPaint + "&";

            return result;
        }

        public void LoadFromStr(string[] array)
        {
            Name = array[0];

            Color = Color.FromArgb(Convert.ToInt32(array[1]));

            Enum.TryParse(array[2], out ChartPaintType);

            IsPaint = Convert.ToBoolean(array[3]);
        }

        public void Dispose()
        {
            Values.Clear();
            Values = null;
            Name = null;
            NameSeries = null;
        }
    }

    public enum IndicatorState
    {
        Configure,
        Dispose,
    }
}