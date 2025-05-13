/*

   Copyright 2025 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections;
using System.Collections.Generic;
using ActiproSoftware.Text;
using ActiproSoftware.Text.Tagging;
using ActiproSoftware.Text.Utility;

#if WINRT
using ActiproSoftware.UI.Xaml.Controls.SyntaxEditor.Adornments;
using ActiproSoftware.UI.Xaml.Controls.SyntaxEditor.IntelliPrompt;

namespace ActiproSoftware.UI.Xaml.Controls.SyntaxEditor.Implementation {
#elif WINFORMS
using ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.Adornments;
using ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.IntelliPrompt;

namespace ActiproSoftware.UI.WinForms.Controls.SyntaxEditor.Implementation {
#elif WPF
using ActiproSoftware.Windows.Controls.SyntaxEditor.Adornments;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt;

namespace ActiproSoftware.Windows.Controls.SyntaxEditor.Implementation {
#endif

/// <summary>
/// Manages the known services for a <see cref="SyntaxEditor"/>.
/// </summary>
internal class ServiceManager
{

  private ISyntaxLanguage cacheLanguage;
  private Dictionary<Type, IEnumerable> sortedNonViewCache = new Dictionary<Type, IEnumerable>();
  private Dictionary<ITextView, Dictionary<Type, IEnumerable>> sortedViewCaches = new Dictionary<ITextView, Dictionary<Type, IEnumerable>>();
  private SyntaxEditor syntaxEditor;
  private Dictionary<Type, IEnumerable> unsortedNonViewCache = new Dictionary<Type, IEnumerable>();
  private Dictionary<ITextView, Dictionary<Type, IEnumerable>> unsortedViewCaches = new Dictionary<ITextView, Dictionary<Type, IEnumerable>>();

  /////////////////////////////////////////////////////////////////////////////////////////////////////
  // OBJECT
  /////////////////////////////////////////////////////////////////////////////////////////////////////

  /// <summary>
  /// Initializes a new instance of the <c>ServiceManager</c> class. 
  /// </summary>
  /// <param name="syntaxEditor">The <see cref="SyntaxEditor"/> for which to manage services.</param>
  public ServiceManager(SyntaxEditor syntaxEditor)
  {
    if (syntaxEditor == null)
      throw new ArgumentNullException("syntaxEditor");

    // Initialize
    this.syntaxEditor = syntaxEditor;
  }

  /////////////////////////////////////////////////////////////////////////////////////////////////////
  // NON-PUBLIC PROCEDURES
  /////////////////////////////////////////////////////////////////////////////////////////////////////

  /// <summary>
  /// Appends the available services of the specified type to a collection of services.
  /// </summary>
  /// <param name="services">The list of services.</param>
  /// <param name="language">The <see cref="ISyntaxLanguage"/> to examine for services.</param>
  /// <typeparam name="TService">The type of service.</typeparam>
  private static void AppendServicesForLanguage<TService>(List<TService> services, ISyntaxLanguage language) where TService : class
  {
    if (language != null)
    {
      // Give priority to exact service type matches
      var service = language.GetService<TService>();
      if (service != null)
        services.Add(service);

      lock (language.SyncRoot)
      {
        // Find language services that match the requested service type
        foreach (var languageServiceType in language.GetAllServiceTypes())
        {
          if (languageServiceType as Type != typeof(TService))
          {
            service = language.GetService(languageServiceType) as TService;
            if ((service != null) && (!services.Contains(service)))
              services.Add(service);
          }
        }
      }
    }
  }

  /// <summary>
  /// Appends the available services of the specified type to a collection of services.
  /// </summary>
  /// <param name="services">The list of services.</param>
  /// <typeparam name="TService">The type of service.</typeparam>
  private void AppendServicesForSyntaxEditor<TService>(List<TService> services) where TService : class
  {
    // Add incremental search if it matches the requested service type
    var service = syntaxEditor.IncrementalSearch as TService;
    if (service != null)
      services.Add(service);

    // Add open IntelliPrompt sessions with the requested service... newer sessions take priority in general
    var intelliPromptManager = syntaxEditor.IntelliPrompt;
    if (intelliPromptManager != null)
    {
      var sessions = intelliPromptManager.Sessions;
      if (sessions != null)
      {
        for (var index = sessions.Count - 1; index >= 0; index--)
        {
          var session = sessions[index];
          if ((session != null) && (session.IsOpen))
          {
            // Give priority to exact service type matches
            service = session.GetService<TService>();
            if (service != null)
              services.Add(service);

            lock (session.SyncRoot)
            {
              // Find language services that match the requested service type
              foreach (var languageServiceType in session.GetAllServiceTypes())
              {
                if (languageServiceType as Type != typeof(TService))
                {
                  service = session.GetService(languageServiceType) as TService;
                  if ((service != null) && (!services.Contains(service)))
                    services.Add(service);
                }
              }
            }
          }
        }
      }
    }

    // Add the outlining manager if it matches the requested service type
    var document = syntaxEditor.Document;
    if (document != null)
    {
      service = document.OutliningManager as TService;
      if (service != null)
        services.Add(service);
    }
  }

  /// <summary>
  /// Appends the available services of the specified type to a collection of services.
  /// </summary>
  /// <param name="services">The list of services.</param>
  /// <param name="view">The <see cref="ITextView"/> to examine for services.</param>
  /// <typeparam name="TService">The type of service.</typeparam>
  private static void AppendServicesForView<TService>(List<TService> services, ITextView view) where TService : class
  {
    if (view != null)
    {
      // Add the view if it matches the requested service type
      var service = view as TService;
      if (service != null)
        services.Add(service);
    }
  }

  /// <summary>
  /// Returns the collection of available services of the specified type.
  /// </summary>
  /// <param name="sort">Whether to sort the services.</param>
  /// <param name="view">The view from which to possibly locate services.</param>
  /// <param name="language">The <see cref="ISyntaxLanguage"/> to examine for services.</param>
  /// <typeparam name="TService">The type of service.</typeparam>
  /// <returns>The collection of available services of the specified type.</returns>
  private IEnumerable<TService> GetServices<TService>(bool sort, ITextView view, ISyntaxLanguage language) where TService : class
  {
    // Invalidate the cache if looking at a different language
    if (cacheLanguage != language)
      this.InvalidateAll();

    // Get the cache to use
    var cache = (sort ? sortedNonViewCache : unsortedNonViewCache);
    if (view != null)
    {
      if (sort)
      {
        if (!sortedViewCaches.TryGetValue(view, out cache))
        {
          cache = new Dictionary<Type, IEnumerable>();
          sortedViewCaches[view] = cache;
        }
      }
      else
      {
        if (!unsortedViewCaches.TryGetValue(view, out cache))
        {
          cache = new Dictionary<Type, IEnumerable>();
          unsortedViewCaches[view] = cache;
        }
      }
    }

    // First look in the cache for the service type
    List<TService> services = null;
    var serviceType = typeof(TService);
    IEnumerable servicesEnumerable;
    if (!cache.TryGetValue(serviceType, out servicesEnumerable))
    {
      // Store the cache language
      cacheLanguage = language;

      // Query the services
      services = new List<TService>();
      this.AppendServicesForSyntaxEditor<TService>(services);
      if (view != null)
        AppendServicesForView<TService>(services, view);
      AppendServicesForLanguage<TService>(services, language);

      // Sort the services
      if (sort)
        services = this.SortServices(services);

      // Cache the results
      cache[serviceType] = services;
    }
    else
      services = servicesEnumerable as List<TService>;

    return services;
  }

  /// <summary>
  /// Occurs when the services on the current language have changed.
  /// </summary>
  /// <param name="sender">The sender of the event.</param>
  /// <param name="e">The <c>CollectionChangeEventArgs</c> that contains data related to this event.</param>
  private void OnLanguageServicesChanged(object sender, CollectionChangeEventArgs<object> e)
  {
    this.InvalidateAll();
  }

  /// <summary>
  /// Sorts the specified services list.
  /// </summary>
  /// <param name="services">The list of services to sort.</param>
  /// <returns>The sorted list.</returns>
  private List<TService> SortServices<TService>(List<TService> services)
  {
    // Create a pre-comparison to ensure IntelliPrompt sessions remain at the start
    Comparison<TService> preComparison = (left, right) =>
    {
      var sessionLeft = left as IIntelliPromptSession;
      var sessionRight = right as IIntelliPromptSession;

      if (sessionLeft != null)
      {
        // Force the left to show first if the right is not a session
        return (sessionRight != null ? 0 : -1);
      }
      else if (sessionRight != null)
      {
        // Force right to show first
        return 1;
      }
      else
      {
        // Neither are sessions... fall back to default comparison
        return 0;
      }
    };

    // Create a fallback-comparison to ensure IntelliPrompt sessions are sorted by their reverse session order
    Comparison<TService> fallbackComparison = (left, right) =>
    {
      var intelliPromptManager = syntaxEditor.IntelliPrompt;
      if (intelliPromptManager != null)
      {
        var sessions = intelliPromptManager.Sessions;
        if ((sessions != null) && (sessions.Count > 0))
        {
          var sessionLeft = left as IIntelliPromptSession;
          var sessionRight = right as IIntelliPromptSession;
          if ((sessionLeft != null) && (sessionRight != null))
          {
            var sessionLeftIndex = sessions.IndexOf(sessionLeft);
            var sessionRightIndex = sessions.IndexOf(sessionRight);
            if ((sessionLeftIndex != -1) && (sessionRightIndex != -1))
              return sessionRightIndex.CompareTo(sessionLeftIndex);
          }
        }
      }

      return 0;
    };

    // Sort
    services = new List<TService>(OrderableHelper.Sort(services, preComparison, fallbackComparison));

    return services;
  }

  /////////////////////////////////////////////////////////////////////////////////////////////////////
  // PUBLIC PROCEDURES
  /////////////////////////////////////////////////////////////////////////////////////////////////////

  /// <summary>
  /// Attaches the service manager to a language.
  /// </summary>
  /// <param name="language">The <see cref="ISyntaxLanguage"/> that was attached.</param>
  public void AttachLanguage(ISyntaxLanguage language)
  {
    // Attach to events
    if (language != null)
    {
      language.ServiceAdded += this.OnLanguageServicesChanged;
      language.ServiceRemoved += this.OnLanguageServicesChanged;
    }

    this.InvalidateAll();
  }

  /// <summary>
  /// Detaches the service manager from a language.
  /// </summary>
  /// <param name="language">The <see cref="ISyntaxLanguage"/> that was detached.</param>
  public void DetachLanguage(ISyntaxLanguage language)
  {
    // Detach from events
    if (language != null)
    {
      language.ServiceAdded -= this.OnLanguageServicesChanged;
      language.ServiceRemoved -= this.OnLanguageServicesChanged;
    }

    this.InvalidateAll();
  }

  /// <summary>
  /// Invalidates the entire cache of services.
  /// </summary>
  public void InvalidateAll()
  {
    cacheLanguage = null;
    sortedNonViewCache.Clear();
    sortedViewCaches.Clear();
    unsortedNonViewCache.Clear();
    unsortedViewCaches.Clear();
  }

  /// <summary>
  /// Returns the collection of available services of the specified type.
  /// </summary>
  /// <param name="sort">Whether to sort the services.</param>
  /// <param name="view">The view from which to possibly locate services.</param>
  /// <typeparam name="TService">The type of service.</typeparam>
  /// <returns>The collection of available services of the specified type.</returns>
  public IEnumerable<TService> GetServices<TService>(bool sort, ITextView view = null) where TService : class
  {
    var language = (syntaxEditor.Document != null ? syntaxEditor.Document.Language : null);
    return this.GetServices<TService>(sort, view, language);
  }

  /// <summary>
  /// Notifies that a language was attached.
  /// </summary>
  /// <param name="language">The <see cref="ISyntaxLanguage"/> that was attached.</param>
  public void NotifyLanguageAttached(ISyntaxLanguage language)
  {
    // Notify of a lifecycle change to attach to the views in this editor
    var viewManager = syntaxEditor.ViewManager;
    if (viewManager != null)
    {
      var adornmentManagerProviders = this.GetServices<IAdornmentManagerProvider>(true, null, language);
      var eventSinks = this.GetServices<ITextViewLifecycleEventSink>(true, null, language);
      foreach (var view in viewManager.Views)
      {
        foreach (var provider in adornmentManagerProviders)
          provider.GetAdornmentManager(view);
        foreach (var eventSink in eventSinks)
          eventSink.NotifyViewAttached(view);
      }
    }
  }

  /// <summary>
  /// Notifies that a language was detached.
  /// </summary>
  /// <param name="language">The <see cref="ISyntaxLanguage"/> that was detached.</param>
  public void NotifyLanguageDetached(ISyntaxLanguage language)
  {
    // Notify of a lifecycle change to detach from the views in this editor
    var viewManager = syntaxEditor.ViewManager;
    if (viewManager != null)
    {
      var eventSinks = this.GetServices<ITextViewLifecycleEventSink>(true, null, language);
      foreach (var view in viewManager.Views)
      {
        foreach (var eventSink in eventSinks)
          eventSink.NotifyViewDetached(view);
      }
    }
  }

  /// <summary>
  /// Notifies that a <see cref="ITextView"/> is attached.
  /// </summary>
  /// <param name="view">The <see cref="ITextView"/> that was attached.</param>
  public void NotifyViewAttached(ITextView view)
  {
    // Notify of a lifecycle change to attach to the views in this editor
    var viewManager = syntaxEditor.ViewManager;
    if (viewManager != null)
    {
      var adornmentManagerProviders = this.GetServices<IAdornmentManagerProvider>(true);
      if (adornmentManagerProviders != null)
      {
        foreach (var provider in adornmentManagerProviders)
          provider.GetAdornmentManager(view);
      }

      var eventSinks = this.GetServices<ITextViewLifecycleEventSink>(true);
      if (eventSinks != null)
      {
        foreach (var eventSink in eventSinks)
          eventSink.NotifyViewAttached(view);
      }
    }
  }

  /// <summary>
  /// Notifies that a <see cref="ITextView"/> is detached.
  /// </summary>
  /// <param name="view">The <see cref="ITextView"/> that was detached.</param>
  public void NotifyViewDetached(ITextView view)
  {
    // Remove any cached information for the view
    sortedViewCaches.Remove(view);
    unsortedViewCaches.Remove(view);

    // Notify of a lifecycle change to attach to the views in this editor
    var viewManager = syntaxEditor.ViewManager;
    if (viewManager != null)
    {
      var eventSinks = this.GetServices<ITextViewLifecycleEventSink>(true);
      if (eventSinks != null)
      {
        foreach (var eventSink in eventSinks)
          eventSink.NotifyViewDetached(view);
      }
    }
  }

}

