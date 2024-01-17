
//------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by Speakeasy (https://speakeasyapi.dev). DO NOT EDIT.
//
// Changes to this file may cause incorrect behavior and will be lost when
// the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable
namespace HathoraCloud.Models.Shared
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System;
    using UnityEngine;
    
    /// <summary>
    /// A process object represents a runtime instance of your game server and its metadata.
    /// </summary>
    [Serializable]
    public class Process
    {

        /// <summary>
        /// Tracks the number of active connections to a process.
        /// </summary>
        [SerializeField]
        [JsonProperty("activeConnections")]
        public double ActiveConnections { get; set; } = default!;
        

        [Obsolete("This field will be removed in a future release, please migrate away from it as soon as possible")]
        [SerializeField]
        [JsonProperty("activeConnectionsUpdatedAt")]
        public DateTime ActiveConnectionsUpdatedAt { get; set; } = default!;
        

        [SerializeField]
        [JsonProperty("additionalExposedPorts")]
        public List<ExposedPort> AdditionalExposedPorts { get; set; } = default!;
        

        /// <summary>
        /// System generated unique identifier for an application.
        /// </summary>
        [SerializeField]
        [JsonProperty("appId")]
        public string AppId { get; set; } = default!;
        

        /// <summary>
        /// System generated id for a deployment. Increments by 1.
        /// </summary>
        [SerializeField]
        [JsonProperty("deploymentId")]
        public int DeploymentId { get; set; } = default!;
        

        /// <summary>
        /// Process in drain will not accept any new rooms.
        /// </summary>
        [SerializeField]
        [JsonProperty("draining")]
        public bool Draining { get; set; } = default!;
        

        /// <summary>
        /// Measures network traffic leaving the process in bytes.
        /// </summary>
        [SerializeField]
        [JsonProperty("egressedBytes")]
        public double EgressedBytes { get; set; } = default!;
        

        /// <summary>
        /// Connection details for an active process.
        /// </summary>
        [SerializeField]
        [JsonProperty("exposedPort")]
        public ExposedPort? ExposedPort { get; set; }
        

        [Obsolete("This field will be removed in a future release, please migrate away from it as soon as possible")]
        [SerializeField]
        [JsonProperty("host")]
        public string Host { get; set; } = default!;
        

        [Obsolete("This field will be removed in a future release, please migrate away from it as soon as possible")]
        [SerializeField]
        [JsonProperty("idleSince")]
        public DateTime IdleSince { get; set; } = default!;
        

        [Obsolete("This field will be removed in a future release, please migrate away from it as soon as possible")]
        [SerializeField]
        [JsonProperty("port")]
        public double Port { get; set; } = default!;
        

        /// <summary>
        /// System generated unique identifier to a runtime instance of your game server.
        /// </summary>
        [SerializeField]
        [JsonProperty("processId")]
        public string ProcessId { get; set; } = default!;
        

        [SerializeField]
        [JsonProperty("region")]
        public Region Region { get; set; } = default!;
        

        [Obsolete("This field will be removed in a future release, please migrate away from it as soon as possible")]
        [SerializeField]
        [JsonProperty("roomSlotsAvailable")]
        public double RoomSlotsAvailable { get; set; } = default!;
        

        [Obsolete("This field will be removed in a future release, please migrate away from it as soon as possible")]
        [SerializeField]
        [JsonProperty("roomSlotsAvailableUpdatedAt")]
        public DateTime RoomSlotsAvailableUpdatedAt { get; set; } = default!;
        

        /// <summary>
        /// Tracks the number of rooms that have been allocated to the process.
        /// </summary>
        [SerializeField]
        [JsonProperty("roomsAllocated")]
        public double RoomsAllocated { get; set; } = default!;
        

        [SerializeField]
        [JsonProperty("roomsAllocatedUpdatedAt")]
        public DateTime RoomsAllocatedUpdatedAt { get; set; } = default!;
        

        /// <summary>
        /// Governs how many <a href="https://hathora.dev/docs/concepts/hathora-entities#room">rooms</a> can be scheduled in a process.
        /// </summary>
        [SerializeField]
        [JsonProperty("roomsPerProcess")]
        public int RoomsPerProcess { get; set; } = default!;
        

        /// <summary>
        /// When the process bound to the specified port. We use this to determine when we should start billing.
        /// </summary>
        [SerializeField]
        [JsonProperty("startedAt")]
        public DateTime StartedAt { get; set; } = default!;
        

        /// <summary>
        /// When the process started being provisioned.
        /// </summary>
        [SerializeField]
        [JsonProperty("startingAt")]
        public DateTime StartingAt { get; set; } = default!;
        

        /// <summary>
        /// When the process is issued to stop. We use this to determine when we should stop billing.
        /// </summary>
        [SerializeField]
        [JsonProperty("stoppingAt")]
        public DateTime StoppingAt { get; set; } = default!;
        

        /// <summary>
        /// When the process has been terminated.
        /// </summary>
        [SerializeField]
        [JsonProperty("terminatedAt")]
        public DateTime TerminatedAt { get; set; } = default!;
        
    }
}