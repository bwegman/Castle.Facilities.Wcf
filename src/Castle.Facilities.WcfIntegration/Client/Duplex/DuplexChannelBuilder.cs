// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Facilities.WcfIntegration
{
	using System;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using Castle.MicroKernel;

    public class DuplexChannelBuilder : AbstractChannelBuilder<DuplexClientModel>
    {
		public DuplexChannelBuilder(IKernel kernel, IChannelFactoryBuilder<DuplexClientModel> channelFactoryBuilder) 
			: base(kernel, channelFactoryBuilder)
        {
        }

        private InstanceContext GetInstanceContext(DuplexClientModel clientModel)
        {
            if (clientModel.CallbackContext == null)
            {
                if (clientModel.CallbackType == null)
                    throw new InvalidOperationException(
                        "Neither CallbackContext nor CallbackType defined in DuplexClientModel");

                var callback = Kernel.Resolve(clientModel.CallbackType);

                if (callback == null)
                    throw new InvalidOperationException(
                        "Unable to resolve callback");

                clientModel.CallbackContext = new InstanceContext(callback);
            }

            return clientModel.CallbackContext;
        }

        protected override ChannelCreator GetChannel(DuplexClientModel clientModel, Type contract, Binding binding, string address)
        {
            return CreateChannelCreator(contract, clientModel, binding, address);
        }

        protected override ChannelCreator GetChannel(DuplexClientModel clientModel, Type contract, Binding binding,
                                                     EndpointAddress address)
        {
            return CreateChannelCreator(contract, clientModel, binding, address);
        }

        protected override ChannelCreator CreateChannelCreator(Type contract, DuplexClientModel clientModel, params object[] args)
        {
            return () =>
                       {
                           var channelFactoryArgs = new object[3];

                           channelFactoryArgs[0] = GetInstanceContext(clientModel);
                           channelFactoryArgs[1] = args[0];
                           channelFactoryArgs[2] = args[1];

                           var type = typeof(DuplexChannelFactory<>).MakeGenericType(new[] { contract });

                           var channelFactory = ChannelFactoryBuilder.CreateChannelFactory(type, clientModel, channelFactoryArgs);
                           ConfigureChannelFactory(channelFactory);

                           var methodInfo = type.GetMethod("CreateChannel", new Type[0]);

                           var channelCreate = (ChannelCreator)Delegate.CreateDelegate(typeof(ChannelCreator), channelFactory, methodInfo);

                           return channelCreate();
                       };
        }
    }
}
