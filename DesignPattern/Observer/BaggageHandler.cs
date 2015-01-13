﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Observer
{
    /// <summary>
    /// 观察者模式中的provider
    /// A BaggageHandler class is responsible for receiving information about arriving flights and baggage claim carousels. 
    /// Internally, it maintains two collections:
    /// observers - A collection of clients that will receive updated information.
    /// flights - A collection of flights and their assigned carousels.
    /// 
    /// Both collections are represented by generic List<T> objects that are instantiated in the BaggageHandler class constructor. 
    /// 
    /// 第一点
    /// A provider or subject, which is the object that sends notifications to observers. 
    /// A provider is a class or structure that implements the IObservable<T> interface.
    /// </summary>
    class BaggageHandler : IObservable<BaggageInfo>
    {
        /// <summary>
        /// 第三点
        /// A mechanism that allows the provider to keep track of observers. 
        /// Typically, the provider uses a container object, such as a System.Collections.Generic.List<T> object, 
        /// to hold references to the IObserver<T> implementations that have subscribed to notifications. 
        /// Using a storage container for this purpose enables the provider to handle zero to an unlimited number of observers. 
        /// 
        /// The order in which observers receive notifications is not defined;
        /// the provider is free to use any method to determine the order.
        /// </summary>
        List<IObserver<BaggageInfo>> observers;

        /// <summary>
        /// 第五点
        /// An object that contains the data that the provider sends to its observers. 
        /// The type of this object corresponds to the generic type parameter of the IObservable<T> and IObserver<T> interfaces. 
        /// Although this object can be the same as the IObservable<T> implementation, most commonly it is a separate type.
        /// </summary>
        List<BaggageInfo> flights;//这里List<T>中的T和IObservable<T>以及IObserver<T>中的T，是同一个类型

        internal BaggageHandler()
        {
            observers = new List<IObserver<BaggageInfo>>();
            flights = new List<BaggageInfo>();
        }

        /// <summary>
        /// 第一点和第四点
        /// The provider must implement a single method, IObservable<T>.Subscribe, 
        /// which is called by observers that wish to receive notifications from the provider.
        /// 
        /// Clients that wish to receive updated information call the BaggageHandler.Subscribe method.
        /// If the client has not previously subscribed to notifications, 
        /// a reference to the client's IObserver<T> implementation is added to the observers collection.
        /// </summary>
        /// <param name="observer"></param>
        /// <returns>The provider's Subscribe method returns an IDisposable implementation that enables observers to stop receiving notifications before the OnCompleted method is called. </returns>
        internal IDisposable Subscribe(IObserver<BaggageInfo> observer)
        {
            //检查observer是否已经添加到集合中，如果没有就添加
            if (observers.Contains(observer) == false)
            {
                observers.Add(observer);
                //向observer提供存在的数据
                foreach (var item in flights)
                {
                    observer.OnNext(item);
                }
            }
            //第四点
            //An IDisposable implementation that enables the provider to remove observers when notification is complete.
            //Observers receive a reference to the IDisposable implementation from the Subscribe method, 
            //so they can also call the IDisposable.Dispose method to unsubscribe before the provider has finished sending notifications.
            return new Unsubscriber<BaggageInfo>(observers, observer);
            //When the class is instantiated in the BaggageHandler.Subscribe method, it is passed a reference to the observers collection and a reference to the observer that is added to the collection. These references are assigned to local variables. 
        }

        /// <summary>
        /// The overloaded BaggageHandler.BaggageStatus method can be called to indicate that baggage from a flight either is being unloaded or is no longer being unloaded.  
        /// In the first case, the method is passed a flight number, the airport from which the flight originated, and the carousel where baggage is being unloaded. 
        /// </summary>
        /// <param name="flight"></param>
        /// <param name="from"></param>
        /// <param name="carousel"></param>
        internal void BaggageStatus(int flight, string from, int carousel)
        {
            var info = new BaggageInfo(flight, from, carousel);
            
            if (carousel > 0 && flights.Contains(info) == false)
            {   
                // Carousel is assigned, so add new info object to list. 
                flights.Add(info);
                foreach (var item in observers)
                {
                    item.OnNext(info);
                }
            }
            else if (carousel == 0)
            {
                // Baggage claim for flight is done 
                var flightsToRemove = new List<BaggageInfo>();
                foreach (var flightItem in flights)
                {
                    if (flightItem.FlightNumber == flight)
                    {
                        flightsToRemove.Add(flightItem);
                        foreach (var observerItem in observers)
                        {
                            observerItem.OnNext(info);
                        }
                    }
                }
                foreach (var flightToRemove in flightsToRemove)
                {
                    flights.Remove(flightToRemove);
                }
                flightsToRemove.Clear();
            }
        }

        /// <summary>
        /// Called to indicate all baggage is now unloaded. 
        /// 
        /// The overloaded BaggageHandler.BaggageStatus method can be called to indicate that baggage from a flight either is being unloaded or is no longer being unloaded.  
        /// In the second case, the method is passed only a flight number. 
        /// For baggage that is being unloaded, the method checks whether the BaggageInfo information passed to the method exists in the flights collection. 
        /// If it does not, the method adds the information and calls each observer's OnNext method.
        /// For flights whose baggage is no longer being unloaded, the method checks whether information on that flight is stored in the flights collection. 
        /// If it is, the method calls each observer's OnNext method and removes the BaggageInfo object from the flights collection.
        /// </summary>
        /// <param name="flight"></param>
        internal void BaggageStatus(int flight)
        {
            BaggageStatus(flight, string.Empty, 0);
        }

        /// <summary>
        /// When the last flight of the day has landed and its baggage has been processed,
        /// the BaggageHandler.LastBaggageClaimed method is called.
        /// 
        /// This method calls each observer's OnCompleted method to indicate that all notifications have completed,
        /// and then clears the observers collection.
        /// </summary>
        internal void LastBaggageClaimed()
        {
            foreach (var item in observers)
            {
                item.OnCompleted();
            }
            observers.Clear();
        }
    }
}
