﻿using BIT.Data.DataTransfer;
using BIT.Data.Services;
using BIT.Data.Xpo.Models;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace BIT.Data.Xpo.Functions
{



   
    public class DataStoreFunctionServer : IFunction
    {
        public IConfigResolver<IDataStore> ConfigResolver { get; set; }
        public IObjectSerializationService ObjectSerializationService { get; set; }
        public DataStoreFunctionServer(IConfigResolver<IDataStore> configResolver, IObjectSerializationService objectSerializationService)
        {
            ConfigResolver = configResolver;
            ObjectSerializationService = objectSerializationService;
        }
        public DataStoreFunctionServer(string ConfigName)
        {
            this.ConfigResolver = new XpoDataStoreResolver(ConfigName);
        }
        public DataStoreFunctionServer()
        {
            this.ConfigResolver = new XpoDataStoreResolver();
        }
        public Task<IDataResult> ExecuteFunction(IDataParameters Parameters)
        {
            DataResult dataResult = new DataResult();
            var DataStore = this.ConfigResolver.GetById(Parameters.AdditionalValues["Id"].ToString());
            if (Parameters.MemberName == nameof(IDataStore.SelectData))
            {

                dataResult.ResultValue =
                    ObjectSerializationService
                    .ToByteArray(
                        DataStore.SelectData(
                            ObjectSerializationService
                            .GetObjectsFromByteArray<SelectStatement[]>(Parameters.ParametersValue)
                            )
                        );
            }
            if (Parameters.MemberName == nameof(IDataStore.ModifyData))
            {
                dataResult.ResultValue =
                   ObjectSerializationService
                   .ToByteArray(
                       DataStore.ModifyData(
                           ObjectSerializationService
                           .GetObjectsFromByteArray<ModificationStatement[]>(Parameters.ParametersValue)
                           )
                       );
            }
            if (Parameters.MemberName == nameof(IDataStore.UpdateSchema))
            {
                UpdateSchemaParameters updateSchemaParameters = ObjectSerializationService
                          .GetObjectsFromByteArray<UpdateSchemaParameters>(Parameters.ParametersValue);
                dataResult.ResultValue =
                  ObjectSerializationService
                  .ToByteArray(
                      DataStore.UpdateSchema(updateSchemaParameters.dontCreateIfFirstTableNotExist,
                      updateSchemaParameters.tables)
                      );
            }
            if (Parameters.MemberName == nameof(ICommandChannel.Do))
            {
                CommandChannelDoParams DoParams = ObjectSerializationService
                          .GetObjectsFromByteArray<CommandChannelDoParams>(Parameters.ParametersValue);

                ICommandChannel commandChannel = DataStore as ICommandChannel;
                if (commandChannel != null)
                {
                    dataResult.ResultValue =
                                 ObjectSerializationService
                                 .ToByteArray(
                                     commandChannel.Do(DoParams.Command,
                                     DoParams.Args)
                                     );
                }

            }

            return Task.FromResult<IDataResult>(dataResult);
        }
    }
}
