// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: EntityState.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Game {

  /// <summary>Holder for reflection information generated from EntityState.proto</summary>
  public static partial class EntityStateReflection {

    #region Descriptor
    /// <summary>File descriptor for EntityState.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static EntityStateReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChFFbnRpdHlTdGF0ZS5wcm90bxIEZ2FtZSI9CgtFbnRpdHlTdGF0ZRIOCgZo",
            "ZWFsdGgYASABKAUSDgoGYWN0aXZlGAIgASgIEg4KBnBvaW50cxgDIAEoBWIG",
            "cHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Game.EntityState), global::Game.EntityState.Parser, new[]{ "Health", "Active", "Points" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class EntityState : pb::IMessage<EntityState>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<EntityState> _parser = new pb::MessageParser<EntityState>(() => new EntityState());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<EntityState> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Game.EntityStateReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public EntityState() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public EntityState(EntityState other) : this() {
      health_ = other.health_;
      active_ = other.active_;
      points_ = other.points_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public EntityState Clone() {
      return new EntityState(this);
    }

    /// <summary>Field number for the "health" field.</summary>
    public const int HealthFieldNumber = 1;
    private int health_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Health {
      get { return health_; }
      set {
        health_ = value;
      }
    }

    /// <summary>Field number for the "active" field.</summary>
    public const int ActiveFieldNumber = 2;
    private bool active_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Active {
      get { return active_; }
      set {
        active_ = value;
      }
    }

    /// <summary>Field number for the "points" field.</summary>
    public const int PointsFieldNumber = 3;
    private int points_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Points {
      get { return points_; }
      set {
        points_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as EntityState);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(EntityState other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Health != other.Health) return false;
      if (Active != other.Active) return false;
      if (Points != other.Points) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Health != 0) hash ^= Health.GetHashCode();
      if (Active != false) hash ^= Active.GetHashCode();
      if (Points != 0) hash ^= Points.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (Health != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Health);
      }
      if (Active != false) {
        output.WriteRawTag(16);
        output.WriteBool(Active);
      }
      if (Points != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(Points);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (Health != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Health);
      }
      if (Active != false) {
        output.WriteRawTag(16);
        output.WriteBool(Active);
      }
      if (Points != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(Points);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Health != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Health);
      }
      if (Active != false) {
        size += 1 + 1;
      }
      if (Points != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Points);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(EntityState other) {
      if (other == null) {
        return;
      }
      if (other.Health != 0) {
        Health = other.Health;
      }
      if (other.Active != false) {
        Active = other.Active;
      }
      if (other.Points != 0) {
        Points = other.Points;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Health = input.ReadInt32();
            break;
          }
          case 16: {
            Active = input.ReadBool();
            break;
          }
          case 24: {
            Points = input.ReadInt32();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 8: {
            Health = input.ReadInt32();
            break;
          }
          case 16: {
            Active = input.ReadBool();
            break;
          }
          case 24: {
            Points = input.ReadInt32();
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
