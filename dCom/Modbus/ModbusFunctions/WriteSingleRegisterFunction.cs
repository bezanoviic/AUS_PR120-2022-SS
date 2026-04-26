using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write single register functions/requests.
    /// </summary>
    public class WriteSingleRegisterFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleRegisterFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleRegisterFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters parameters = (ModbusWriteCommandParameters)CommandParameters;

            byte[] request = new byte[12];

            request[0] = (byte)(parameters.TransactionId >> 8);
            request[1] = (byte)(parameters.TransactionId & 0xFF);

            request[2] = 0;
            request[3] = 0;

            request[4] = (byte)(parameters.Length >> 8);
            request[5] = (byte)(parameters.Length & 0xFF);

            request[6] = parameters.UnitId;
            request[7] = parameters.FunctionCode;

            request[8] = (byte)(parameters.OutputAddress >> 8);
            request[9] = (byte)(parameters.OutputAddress & 0xFF);

            request[10] = (byte)(parameters.Value >> 8);
            request[11] = (byte)(parameters.Value & 0xFF);

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusWriteCommandParameters parameters = (ModbusWriteCommandParameters)CommandParameters;

            byte functionCode = response[7];

            if ((functionCode & 0x80) != 0)
            {
                HandeException(response[8]);
            }

            ushort value = (ushort)((response[10] << 8) | response[11]);

            result.Add(
                new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, parameters.OutputAddress),
                value);

            return result;
        }
    }
}