// automatically generated by the FlatBuffers compiler, do not modify

namespace Iris.Serialization.Raft
{

using System;
using FlatBuffers;

public sealed class AddIOBoxFB : Table {
  public static AddIOBoxFB GetRootAsAddIOBoxFB(ByteBuffer _bb) { return GetRootAsAddIOBoxFB(_bb, new AddIOBoxFB()); }
  public static AddIOBoxFB GetRootAsAddIOBoxFB(ByteBuffer _bb, AddIOBoxFB obj) { return (obj.__init(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public AddIOBoxFB __init(int _i, ByteBuffer _bb) { bb_pos = _i; bb = _bb; return this; }

  public IOBoxFB IOBox { get { return GetIOBox(new IOBoxFB()); } }
  public IOBoxFB GetIOBox(IOBoxFB obj) { int o = __offset(4); return o != 0 ? obj.__init(__indirect(o + bb_pos), bb) : null; }

  public static Offset<AddIOBoxFB> CreateAddIOBoxFB(FlatBufferBuilder builder,
      Offset<IOBoxFB> IOBoxOffset = default(Offset<IOBoxFB>)) {
    builder.StartObject(1);
    AddIOBoxFB.AddIOBox(builder, IOBoxOffset);
    return AddIOBoxFB.EndAddIOBoxFB(builder);
  }

  public static void StartAddIOBoxFB(FlatBufferBuilder builder) { builder.StartObject(1); }
  public static void AddIOBox(FlatBufferBuilder builder, Offset<IOBoxFB> IOBoxOffset) { builder.AddOffset(0, IOBoxOffset.Value, 0); }
  public static Offset<AddIOBoxFB> EndAddIOBoxFB(FlatBufferBuilder builder) {
    int o = builder.EndObject();
    return new Offset<AddIOBoxFB>(o);
  }
};


}