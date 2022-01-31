// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: PlayerInputUpdate.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Game {

  /// <summary>Holder for reflection information generated from PlayerInputUpdate.proto</summary>
  public static partial class PlayerInputUpdateReflection {

    #region Descriptor
    /// <summary>File descriptor for PlayerInputUpdate.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static PlayerInputUpdateReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChdQbGF5ZXJJbnB1dFVwZGF0ZS5wcm90bxIER2FtZSIqCgdNVmVjdG9yEgkK",
            "AVgYASABKAISCQoBWRgCIAEoAhIJCgFaGAMgASgCIjkKC01RdWF0ZXJuaW9u",
            "EgkKAVgYASABKAISCQoBWRgCIAEoAhIJCgFaGAMgASgCEgkKAVcYBCABKAIi",
            "pQIKEVBsYXllcklucHV0VXBkYXRlEi0KFmNhbWVyYUZvcndhcmREaXJlY3Rp",
            "b24YASABKAsyDS5HYW1lLk1WZWN0b3ISJgoPdGFyZ2V0RGlyZWN0aW9uGAIg",
            "ASgLMg0uR2FtZS5NVmVjdG9yEiYKD21vdmluZ0RpcmVjdGlvbhgDIAEoCzIN",
            "LkdhbWUuTVZlY3RvchInChBmb3J3YXJkRGlyZWN0aW9uGAQgASgLMg0uR2Ft",
            "ZS5NVmVjdG9yEiYKC2FpbVJvdGF0aW9uGAUgASgLMhEuR2FtZS5NUXVhdGVy",
            "bmlvbhIcChRtb3ZlbWVudElucHV0T25GcmFtZRgGIAEoCBIPCgdkb0lucHV0",
            "GAcgASgIEhEKCXRha2VJbnB1dBgIIAEoCGIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Game.MVector), global::Game.MVector.Parser, new[]{ "X", "Y", "Z" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Game.MQuaternion), global::Game.MQuaternion.Parser, new[]{ "X", "Y", "Z", "W" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Game.PlayerInputUpdate), global::Game.PlayerInputUpdate.Parser, new[]{ "CameraForwardDirection", "TargetDirection", "MovingDirection", "ForwardDirection", "AimRotation", "MovementInputOnFrame", "DoInput", "TakeInput" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class MVector : pb::IMessage<MVector>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<MVector> _parser = new pb::MessageParser<MVector>(() => new MVector());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<MVector> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Game.PlayerInputUpdateReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public MVector() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public MVector(MVector other) : this() {
      x_ = other.x_;
      y_ = other.y_;
      z_ = other.z_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public MVector Clone() {
      return new MVector(this);
    }

    /// <summary>Field number for the "X" field.</summary>
    public const int XFieldNumber = 1;
    private float x_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float X {
      get { return x_; }
      set {
        x_ = value;
      }
    }

    /// <summary>Field number for the "Y" field.</summary>
    public const int YFieldNumber = 2;
    private float y_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Y {
      get { return y_; }
      set {
        y_ = value;
      }
    }

    /// <summary>Field number for the "Z" field.</summary>
    public const int ZFieldNumber = 3;
    private float z_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Z {
      get { return z_; }
      set {
        z_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as MVector);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(MVector other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(X, other.X)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Y, other.Y)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Z, other.Z)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (X != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(X);
      if (Y != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Y);
      if (Z != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Z);
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (X != 0F) {
        output.WriteRawTag(13);
        output.WriteFloat(X);
      }
      if (Y != 0F) {
        output.WriteRawTag(21);
        output.WriteFloat(Y);
      }
      if (Z != 0F) {
        output.WriteRawTag(29);
        output.WriteFloat(Z);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (X != 0F) {
        output.WriteRawTag(13);
        output.WriteFloat(X);
      }
      if (Y != 0F) {
        output.WriteRawTag(21);
        output.WriteFloat(Y);
      }
      if (Z != 0F) {
        output.WriteRawTag(29);
        output.WriteFloat(Z);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (X != 0F) {
        size += 1 + 4;
      }
      if (Y != 0F) {
        size += 1 + 4;
      }
      if (Z != 0F) {
        size += 1 + 4;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(MVector other) {
      if (other == null) {
        return;
      }
      if (other.X != 0F) {
        X = other.X;
      }
      if (other.Y != 0F) {
        Y = other.Y;
      }
      if (other.Z != 0F) {
        Z = other.Z;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
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
          case 13: {
            X = input.ReadFloat();
            break;
          }
          case 21: {
            Y = input.ReadFloat();
            break;
          }
          case 29: {
            Z = input.ReadFloat();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 13: {
            X = input.ReadFloat();
            break;
          }
          case 21: {
            Y = input.ReadFloat();
            break;
          }
          case 29: {
            Z = input.ReadFloat();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class MQuaternion : pb::IMessage<MQuaternion>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<MQuaternion> _parser = new pb::MessageParser<MQuaternion>(() => new MQuaternion());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<MQuaternion> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Game.PlayerInputUpdateReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public MQuaternion() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public MQuaternion(MQuaternion other) : this() {
      x_ = other.x_;
      y_ = other.y_;
      z_ = other.z_;
      w_ = other.w_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public MQuaternion Clone() {
      return new MQuaternion(this);
    }

    /// <summary>Field number for the "X" field.</summary>
    public const int XFieldNumber = 1;
    private float x_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float X {
      get { return x_; }
      set {
        x_ = value;
      }
    }

    /// <summary>Field number for the "Y" field.</summary>
    public const int YFieldNumber = 2;
    private float y_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Y {
      get { return y_; }
      set {
        y_ = value;
      }
    }

    /// <summary>Field number for the "Z" field.</summary>
    public const int ZFieldNumber = 3;
    private float z_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float Z {
      get { return z_; }
      set {
        z_ = value;
      }
    }

    /// <summary>Field number for the "W" field.</summary>
    public const int WFieldNumber = 4;
    private float w_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float W {
      get { return w_; }
      set {
        w_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as MQuaternion);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(MQuaternion other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(X, other.X)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Y, other.Y)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(Z, other.Z)) return false;
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(W, other.W)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (X != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(X);
      if (Y != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Y);
      if (Z != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(Z);
      if (W != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(W);
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (X != 0F) {
        output.WriteRawTag(13);
        output.WriteFloat(X);
      }
      if (Y != 0F) {
        output.WriteRawTag(21);
        output.WriteFloat(Y);
      }
      if (Z != 0F) {
        output.WriteRawTag(29);
        output.WriteFloat(Z);
      }
      if (W != 0F) {
        output.WriteRawTag(37);
        output.WriteFloat(W);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (X != 0F) {
        output.WriteRawTag(13);
        output.WriteFloat(X);
      }
      if (Y != 0F) {
        output.WriteRawTag(21);
        output.WriteFloat(Y);
      }
      if (Z != 0F) {
        output.WriteRawTag(29);
        output.WriteFloat(Z);
      }
      if (W != 0F) {
        output.WriteRawTag(37);
        output.WriteFloat(W);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (X != 0F) {
        size += 1 + 4;
      }
      if (Y != 0F) {
        size += 1 + 4;
      }
      if (Z != 0F) {
        size += 1 + 4;
      }
      if (W != 0F) {
        size += 1 + 4;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(MQuaternion other) {
      if (other == null) {
        return;
      }
      if (other.X != 0F) {
        X = other.X;
      }
      if (other.Y != 0F) {
        Y = other.Y;
      }
      if (other.Z != 0F) {
        Z = other.Z;
      }
      if (other.W != 0F) {
        W = other.W;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
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
          case 13: {
            X = input.ReadFloat();
            break;
          }
          case 21: {
            Y = input.ReadFloat();
            break;
          }
          case 29: {
            Z = input.ReadFloat();
            break;
          }
          case 37: {
            W = input.ReadFloat();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 13: {
            X = input.ReadFloat();
            break;
          }
          case 21: {
            Y = input.ReadFloat();
            break;
          }
          case 29: {
            Z = input.ReadFloat();
            break;
          }
          case 37: {
            W = input.ReadFloat();
            break;
          }
        }
      }
    }
    #endif

  }

  public sealed partial class PlayerInputUpdate : pb::IMessage<PlayerInputUpdate>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PlayerInputUpdate> _parser = new pb::MessageParser<PlayerInputUpdate>(() => new PlayerInputUpdate());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PlayerInputUpdate> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Game.PlayerInputUpdateReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PlayerInputUpdate() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PlayerInputUpdate(PlayerInputUpdate other) : this() {
      cameraForwardDirection_ = other.cameraForwardDirection_ != null ? other.cameraForwardDirection_.Clone() : null;
      targetDirection_ = other.targetDirection_ != null ? other.targetDirection_.Clone() : null;
      movingDirection_ = other.movingDirection_ != null ? other.movingDirection_.Clone() : null;
      forwardDirection_ = other.forwardDirection_ != null ? other.forwardDirection_.Clone() : null;
      aimRotation_ = other.aimRotation_ != null ? other.aimRotation_.Clone() : null;
      movementInputOnFrame_ = other.movementInputOnFrame_;
      doInput_ = other.doInput_;
      takeInput_ = other.takeInput_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PlayerInputUpdate Clone() {
      return new PlayerInputUpdate(this);
    }

    /// <summary>Field number for the "cameraForwardDirection" field.</summary>
    public const int CameraForwardDirectionFieldNumber = 1;
    private global::Game.MVector cameraForwardDirection_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Game.MVector CameraForwardDirection {
      get { return cameraForwardDirection_; }
      set {
        cameraForwardDirection_ = value;
      }
    }

    /// <summary>Field number for the "targetDirection" field.</summary>
    public const int TargetDirectionFieldNumber = 2;
    private global::Game.MVector targetDirection_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Game.MVector TargetDirection {
      get { return targetDirection_; }
      set {
        targetDirection_ = value;
      }
    }

    /// <summary>Field number for the "movingDirection" field.</summary>
    public const int MovingDirectionFieldNumber = 3;
    private global::Game.MVector movingDirection_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Game.MVector MovingDirection {
      get { return movingDirection_; }
      set {
        movingDirection_ = value;
      }
    }

    /// <summary>Field number for the "forwardDirection" field.</summary>
    public const int ForwardDirectionFieldNumber = 4;
    private global::Game.MVector forwardDirection_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Game.MVector ForwardDirection {
      get { return forwardDirection_; }
      set {
        forwardDirection_ = value;
      }
    }

    /// <summary>Field number for the "aimRotation" field.</summary>
    public const int AimRotationFieldNumber = 5;
    private global::Game.MQuaternion aimRotation_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Game.MQuaternion AimRotation {
      get { return aimRotation_; }
      set {
        aimRotation_ = value;
      }
    }

    /// <summary>Field number for the "movementInputOnFrame" field.</summary>
    public const int MovementInputOnFrameFieldNumber = 6;
    private bool movementInputOnFrame_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool MovementInputOnFrame {
      get { return movementInputOnFrame_; }
      set {
        movementInputOnFrame_ = value;
      }
    }

    /// <summary>Field number for the "doInput" field.</summary>
    public const int DoInputFieldNumber = 7;
    private bool doInput_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool DoInput {
      get { return doInput_; }
      set {
        doInput_ = value;
      }
    }

    /// <summary>Field number for the "takeInput" field.</summary>
    public const int TakeInputFieldNumber = 8;
    private bool takeInput_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool TakeInput {
      get { return takeInput_; }
      set {
        takeInput_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PlayerInputUpdate);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PlayerInputUpdate other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(CameraForwardDirection, other.CameraForwardDirection)) return false;
      if (!object.Equals(TargetDirection, other.TargetDirection)) return false;
      if (!object.Equals(MovingDirection, other.MovingDirection)) return false;
      if (!object.Equals(ForwardDirection, other.ForwardDirection)) return false;
      if (!object.Equals(AimRotation, other.AimRotation)) return false;
      if (MovementInputOnFrame != other.MovementInputOnFrame) return false;
      if (DoInput != other.DoInput) return false;
      if (TakeInput != other.TakeInput) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (cameraForwardDirection_ != null) hash ^= CameraForwardDirection.GetHashCode();
      if (targetDirection_ != null) hash ^= TargetDirection.GetHashCode();
      if (movingDirection_ != null) hash ^= MovingDirection.GetHashCode();
      if (forwardDirection_ != null) hash ^= ForwardDirection.GetHashCode();
      if (aimRotation_ != null) hash ^= AimRotation.GetHashCode();
      if (MovementInputOnFrame != false) hash ^= MovementInputOnFrame.GetHashCode();
      if (DoInput != false) hash ^= DoInput.GetHashCode();
      if (TakeInput != false) hash ^= TakeInput.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (cameraForwardDirection_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(CameraForwardDirection);
      }
      if (targetDirection_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(TargetDirection);
      }
      if (movingDirection_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(MovingDirection);
      }
      if (forwardDirection_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(ForwardDirection);
      }
      if (aimRotation_ != null) {
        output.WriteRawTag(42);
        output.WriteMessage(AimRotation);
      }
      if (MovementInputOnFrame != false) {
        output.WriteRawTag(48);
        output.WriteBool(MovementInputOnFrame);
      }
      if (DoInput != false) {
        output.WriteRawTag(56);
        output.WriteBool(DoInput);
      }
      if (TakeInput != false) {
        output.WriteRawTag(64);
        output.WriteBool(TakeInput);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (cameraForwardDirection_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(CameraForwardDirection);
      }
      if (targetDirection_ != null) {
        output.WriteRawTag(18);
        output.WriteMessage(TargetDirection);
      }
      if (movingDirection_ != null) {
        output.WriteRawTag(26);
        output.WriteMessage(MovingDirection);
      }
      if (forwardDirection_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(ForwardDirection);
      }
      if (aimRotation_ != null) {
        output.WriteRawTag(42);
        output.WriteMessage(AimRotation);
      }
      if (MovementInputOnFrame != false) {
        output.WriteRawTag(48);
        output.WriteBool(MovementInputOnFrame);
      }
      if (DoInput != false) {
        output.WriteRawTag(56);
        output.WriteBool(DoInput);
      }
      if (TakeInput != false) {
        output.WriteRawTag(64);
        output.WriteBool(TakeInput);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (cameraForwardDirection_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(CameraForwardDirection);
      }
      if (targetDirection_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(TargetDirection);
      }
      if (movingDirection_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(MovingDirection);
      }
      if (forwardDirection_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(ForwardDirection);
      }
      if (aimRotation_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(AimRotation);
      }
      if (MovementInputOnFrame != false) {
        size += 1 + 1;
      }
      if (DoInput != false) {
        size += 1 + 1;
      }
      if (TakeInput != false) {
        size += 1 + 1;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PlayerInputUpdate other) {
      if (other == null) {
        return;
      }
      if (other.cameraForwardDirection_ != null) {
        if (cameraForwardDirection_ == null) {
          CameraForwardDirection = new global::Game.MVector();
        }
        CameraForwardDirection.MergeFrom(other.CameraForwardDirection);
      }
      if (other.targetDirection_ != null) {
        if (targetDirection_ == null) {
          TargetDirection = new global::Game.MVector();
        }
        TargetDirection.MergeFrom(other.TargetDirection);
      }
      if (other.movingDirection_ != null) {
        if (movingDirection_ == null) {
          MovingDirection = new global::Game.MVector();
        }
        MovingDirection.MergeFrom(other.MovingDirection);
      }
      if (other.forwardDirection_ != null) {
        if (forwardDirection_ == null) {
          ForwardDirection = new global::Game.MVector();
        }
        ForwardDirection.MergeFrom(other.ForwardDirection);
      }
      if (other.aimRotation_ != null) {
        if (aimRotation_ == null) {
          AimRotation = new global::Game.MQuaternion();
        }
        AimRotation.MergeFrom(other.AimRotation);
      }
      if (other.MovementInputOnFrame != false) {
        MovementInputOnFrame = other.MovementInputOnFrame;
      }
      if (other.DoInput != false) {
        DoInput = other.DoInput;
      }
      if (other.TakeInput != false) {
        TakeInput = other.TakeInput;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
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
          case 10: {
            if (cameraForwardDirection_ == null) {
              CameraForwardDirection = new global::Game.MVector();
            }
            input.ReadMessage(CameraForwardDirection);
            break;
          }
          case 18: {
            if (targetDirection_ == null) {
              TargetDirection = new global::Game.MVector();
            }
            input.ReadMessage(TargetDirection);
            break;
          }
          case 26: {
            if (movingDirection_ == null) {
              MovingDirection = new global::Game.MVector();
            }
            input.ReadMessage(MovingDirection);
            break;
          }
          case 34: {
            if (forwardDirection_ == null) {
              ForwardDirection = new global::Game.MVector();
            }
            input.ReadMessage(ForwardDirection);
            break;
          }
          case 42: {
            if (aimRotation_ == null) {
              AimRotation = new global::Game.MQuaternion();
            }
            input.ReadMessage(AimRotation);
            break;
          }
          case 48: {
            MovementInputOnFrame = input.ReadBool();
            break;
          }
          case 56: {
            DoInput = input.ReadBool();
            break;
          }
          case 64: {
            TakeInput = input.ReadBool();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            if (cameraForwardDirection_ == null) {
              CameraForwardDirection = new global::Game.MVector();
            }
            input.ReadMessage(CameraForwardDirection);
            break;
          }
          case 18: {
            if (targetDirection_ == null) {
              TargetDirection = new global::Game.MVector();
            }
            input.ReadMessage(TargetDirection);
            break;
          }
          case 26: {
            if (movingDirection_ == null) {
              MovingDirection = new global::Game.MVector();
            }
            input.ReadMessage(MovingDirection);
            break;
          }
          case 34: {
            if (forwardDirection_ == null) {
              ForwardDirection = new global::Game.MVector();
            }
            input.ReadMessage(ForwardDirection);
            break;
          }
          case 42: {
            if (aimRotation_ == null) {
              AimRotation = new global::Game.MQuaternion();
            }
            input.ReadMessage(AimRotation);
            break;
          }
          case 48: {
            MovementInputOnFrame = input.ReadBool();
            break;
          }
          case 56: {
            DoInput = input.ReadBool();
            break;
          }
          case 64: {
            TakeInput = input.ReadBool();
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
