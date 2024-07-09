using System;
using Python.Runtime;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Bonsai.ML.HiddenMarkovModels.Observations
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AutoRegressiveObservations : ObservationsModel
    {

        /// <summary>
        /// The lags of the observations for each state.
        /// </summary>
        [JsonProperty]
        [Description("The lags of the observations for each state.")]
        public int Lags { get; set; } = 1;

        /// <summary>
        /// The As of the observations for each state.
        /// </summary>
        [Description("The As of the observations for each state.")]
        public double[,,] As { get; private set; } = null;

        /// <summary>
        /// The bs of the observations for each state.
        /// </summary>
        [Description("The bs of the observations for each state.")]
        public double[,] Bs { get; private set; } = null;

        /// <summary>
        /// The Vs of the observations for each state.
        /// </summary>
        [Description("The Vs of the observations for each state.")]
        public double[,,] Vs { get; private set; } = null;

        /// <summary>
        /// The square root sigmas of the observations for each state.
        /// </summary>
        [Description("The square root sigmas of the observations for each state.")]
        public double[,,] SqrtSigmas { get; private set; } = null;

        /// <inheritdoc/>
        [JsonProperty]
        public override ObservationsType ObservationsType => ObservationsType.AutoRegressive;

        /// <inheritdoc/>
        [JsonProperty]
        public override object[] Params
        {
            get { return [ As, Bs, Vs, SqrtSigmas ]; }
            set
            {
                As = (double[,,])value[0];
                Bs = (double[,])value[1];
                Vs = (double[,,])value[2];
                SqrtSigmas = (double[,,])value[3];
            }
        }

        public AutoRegressiveObservations (params object[] args)
        {
            Lags = (int)args[0];
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (As is null || Bs is null || Vs is null || SqrtSigmas is null) 
                return $"observation_params=None,observation_kwargs={{'lags':{Lags}}}";

            return $"observation_params=({NumpyHelper.NumpyParser.ParseArray(As)}," +
                $"{NumpyHelper.NumpyParser.ParseArray(Bs)}," +
                $"{NumpyHelper.NumpyParser.ParseArray(Vs)}," +
                $"{NumpyHelper.NumpyParser.ParseArray(SqrtSigmas)})," +
                $"observation_kwargs={{'lags':{Lags}}}";
        }
    }
}