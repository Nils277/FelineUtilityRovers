/*
 * Copyright (C) 2018 Nils277 (https://github.com/Nils277)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace KerbetrotterTools
{
    public interface IConfigurableResourceModule
    {
        /// <summary>
        /// Adds a listener for resource changes to the list
        /// </summary>
        /// <param name="listener">The new listener</param>
        void addResourceChangeListener(IResourceChangeListener listener);

        /// <summary>
        /// Removes a listener for resource changes from the list
        /// </summary>
        /// <param name="listener">The listener to remove</param>
        void removeResourceChangeListener(IResourceChangeListener listener);
    }
}
