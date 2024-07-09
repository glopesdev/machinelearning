using System.ComponentModel;
using System;
using System.Reactive.Linq;
using Python.Runtime;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Bonsai.ML.HiddenMarkovModels.Observations;
using static Bonsai.ML.HiddenMarkovModels.Observations.ObservationsLookup;
using System.Collections.Generic;
using System.Linq;

namespace Bonsai.ML.HiddenMarkovModels
{

    /// <summary>
    /// StateParameters of a Hidden Markov Model (HMM).
    /// </summary>
    [Combinator]
    [JsonConverter(typeof(StateParametersJsonConverter))]
    [Description("StateParameters of a Hidden Markov Model (HMM).")]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class StateParameters
    {

        private double[] initialStateDistribution;
        private double[,] logTransitionProbabilities;
        private ObservationParams observations;
        private ObservationType observationTypeEnum;

        /// <summary>
        /// The initial state distribution.
        /// </summary>
        [XmlIgnore]
        [JsonProperty("initial_state_distribution")]
        [Description("The initial state distribution.")]
        [Category("ModelStateParameters")]
        public double[] InitialStateDistribution
        {
            get => initialStateDistribution;
            set => initialStateDistribution = value;
        }

        /// <summary>
        /// The log of the state transition probabilities.
        /// </summary>
        [XmlIgnore]
        [JsonProperty("log_transition_probabilities")]
        [Description("The log of the state transition probabilities.")]
        [Category("ModelStateParameters")]
        public double[,] LogTransitionProbabilities
        {
            get => logTransitionProbabilities;
            set => logTransitionProbabilities = value;
        }

        /// <summary>
        /// The observation type.
        /// </summary>
        [XmlIgnore]
        [JsonProperty("observation_type")]
        [JsonConverter(typeof(ObservationTypeJsonConverter))]
        [Description("The observation type.")]
        [Category("ModelStateParameters")]
        public ObservationType ObservationType
        {
            get => observationTypeEnum;
            set => observationTypeEnum = value;
        }

        /// <summary>
        /// The observations.
        /// </summary>
        [XmlIgnore]
        [JsonProperty("observation_params")]
        [Description("The observations.")]
        [Category("ModelStateParameters")]
        public ObservationParams Observations
        {
            get => observations;
            set => observations = value;
        }

        public IObservable<StateParameters> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Select(source, pyObject =>
            {
                return new StateParameters()
                {
                    InitialStateDistribution = InitialStateDistribution,
                    LogTransitionProbabilities = LogTransitionProbabilities,
                    ObservationType = ObservationType,
                    Observations = Observations
                };
            });
        }

        public IObservable<StateParameters> Process(IObservable<PyObject> source)
        {
            return Observable.Select(source, pyObject =>
            {
                var initialStateDistributionPyObj = (double[])pyObject.GetArrayAttr("initial_state_distribution");
                var logTransitionProbabilitiesPyObj = (double[,])pyObject.GetArrayAttr("log_transition_probabilities");
                var observationTypePyObj = pyObject.GetAttr<string>("observation_type");

                var observationsArrayPyObj = (Array)pyObject.GetArrayAttr("observation_params");
                var observationsPyObj = (object[])observationsArrayPyObj;
                var observationKwargsPyObj = (Dictionary<object, object>)pyObject.GetArrayAttr("observation_kwargs");
                var observationConstructors = observationKwargsPyObj.Values.ToArray();

                observationTypeEnum = GetFromString(observationTypePyObj);
                var observationClassType = GetObservationsClassType(observationTypeEnum);

                if (observationConstructors.Length == 0) {
                    observations = (ObservationParams)Activator.CreateInstance(observationClassType);
                }
                else {
                    observations = (ObservationParams)Activator.CreateInstance(observationClassType, observationConstructors);
                }

                observations.Params = observationsPyObj;

                return new StateParameters()
                {
                    InitialStateDistribution = initialStateDistributionPyObj,
                    LogTransitionProbabilities = logTransitionProbabilitiesPyObj,
                    ObservationType = observationTypeEnum,
                    Observations = observations
                };
            });
        }

        public override string ToString()
        {
            return $"initial_state_distribution={NumpyHelper.NumpyParser.ParseArray(InitialStateDistribution)},log_transition_probabilities={NumpyHelper.NumpyParser.ParseArray(LogTransitionProbabilities)},{Observations}";
        }
    }
}
