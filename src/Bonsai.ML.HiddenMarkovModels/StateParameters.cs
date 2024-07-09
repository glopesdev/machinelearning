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
    [WorkflowElementCategory(ElementCategory.Source)]
    public class StateParameters
    {

        private double[] initialStateDistribution = null;
        private double[,] logTransitionProbabilities = null;
        private ObservationsModel observations = null;

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
        /// The observations.
        /// </summary>
        [XmlIgnore]
        [JsonProperty("observation_params")]
        [Description("The observations.")]
        [Category("ModelStateParameters")]
        public ObservationsModel Observations
        {
            get => observations;
            set => observations = value;
        }

        public IObservable<StateParameters> Process()
        {
            return Observable.Return(
                new StateParameters() 
                {
                    InitialStateDistribution = InitialStateDistribution,
                    LogTransitionProbabilities = LogTransitionProbabilities,
                    Observations = Observations
                }
            );
        }

        public IObservable<StateParameters> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Select(source, pyObject =>
            {
                return new StateParameters()
                {
                    InitialStateDistribution = InitialStateDistribution,
                    LogTransitionProbabilities = LogTransitionProbabilities,
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
                var observationsTypePyObj = pyObject.GetAttr<string>("observation_type");

                var observationsArrayPyObj = (Array)pyObject.GetArrayAttr("observation_params");
                var observationsPyObj = (object[])observationsArrayPyObj;
                var observationKwargsPyObj = (Dictionary<object, object>)pyObject.GetArrayAttr("observation_kwargs");
                var observationConstructors = observationKwargsPyObj.Values.ToArray();

                var observationsType = GetFromString(observationsTypePyObj);
                var observationClassType = GetObservationsClassType(observationsType);

                observations = (ObservationsModel)Activator.CreateInstance(observationClassType,
                    observationConstructors.Length == 0 ? null : observationConstructors);

                observations.Params = observationsPyObj;

                return new StateParameters()
                {
                    InitialStateDistribution = initialStateDistributionPyObj,
                    LogTransitionProbabilities = logTransitionProbabilitiesPyObj,
                    Observations = observations
                };
            });
        }

        public override string ToString()
        {
            return $"initial_state_distribution={(InitialStateDistribution == null ? "None" : NumpyHelper.NumpyParser.ParseArray(InitialStateDistribution))}," + 
                $"log_transition_probabilities={(LogTransitionProbabilities == null ? "None" : NumpyHelper.NumpyParser.ParseArray(LogTransitionProbabilities))}," +
                $"{(Observations == null ? "" : Observations)}";
        }
    }
}
