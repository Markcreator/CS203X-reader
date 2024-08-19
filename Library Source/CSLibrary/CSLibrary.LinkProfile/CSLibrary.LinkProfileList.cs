/*
Copyright (c) 2023 Convergence Systems Limited

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/





using System;
using System.Collections.Generic;
using System.Text;



namespace CSLibrary
{
    using CSLibrary.Constants;
    using CSLibrary.Structures;

    internal class LinkProfileInfoList
        :
        List< LinkProfileInfo >
    {
        /**************************************************
         * WARNING -
         * 
         * While link profiles are exposed as a mutable
         * list via this class once loaded from the radio
         * only the active profile should be modified but
         * the held profiles should NEVER BE MODIFIED !!!
         **************************************************/
        //private const int MAX_LINK_PROFILE = 7;
        private const int MAX_LINK_PROFILE = 5;
        private Int32         activeProfileIndex;


        // Force hiding base constructor ~ should never be used

#if nouse
        private LinkProfileInfoList( )
            :
            base( )
        {

        }
#endif

        // Provided just to allow easy copying of existing
        // link profile list source(s)

        public LinkProfileInfoList()
        {
            for ( activeProfileIndex = 0; activeProfileIndex < this.Count; ++activeProfileIndex )
            {
                if ( this[ activeProfileIndex ].Enabled )
                {
                    break;
                }
            }
        }


        // Force hiding base constructor ~ should never be used

        private LinkProfileInfoList( Int32 capacity )
            :
            base( capacity )
        {

        }

	    private void GetLinkProfile(uint profile, RadioLinkProfile pProfileInfo )
	    {
		    uint registerValue;

#if nouse
            // Set the profile selector and verify that it was a valid selector
		    MacWriteRegister (HST_RFTC_PROF_SEL, HST_RFTC_PROF_SEL_PROF(profile) | HST_RFTC_PROF_SEL_RFU1(0));
		
            MacReadRegister(MAC_ERROR, ref registerValue);

            if (registerValue == HOSTIF_ERR_SELECTORBNDS)
		    {
			    MacClearError();
			    return Result.INVALID_PARAMETER;
		    }

		    // Get the config information for the profile
		    registerValue                   = m_pMac->ReadRegister(MAC_RFTC_PROF_CFG);
		    pProfileInfo->enabled           = MAC_RFTC_PROF_CFG_IS_ENABLED(registerValue);
		    pProfileInfo->denseReaderMode   = MAC_RFTC_PROF_CFG_DRM_IS_ENABLED(registerValue);

	    	// Retrieve the profile's ID
		    INT64U profileIdHigh    = m_pMac->ReadRegister(MAC_RFTC_PROF_ID_HIGH);
		    INT64U profileIdLow     = m_pMac->ReadRegister(MAC_RFTC_PROF_ID_LOW);
		    pProfileInfo->profileId = (profileIdHigh << 32) | profileIdLow;

    		// Retrieve the profile's version
	    	pProfileInfo->profileVersion = m_pMac->ReadRegister(MAC_RFTC_PROF_IDVER);

    		// Get the protocol for the link profile and then based upon the protocol,
	    	// Retrieve the appropriate information
		    pProfileInfo->profileProtocol = m_pMac->ReadRegister(MAC_RFTC_PROF_PROTOCOL);

    		// Read the RSSI information for the profile
	    	registerValue                           = m_pMac->ReadRegister(MAC_RFTC_PROF_RSSIAVECFG);
		    pProfileInfo->widebandRssiSamples           = RFID_WIDEBAND_RSSI_BASE_SAMPLES << MAC_RFTC_PROF_RSSIAVECFG_GET_NORM_WBSAMPS(registerValue);
		    pProfileInfo->narrowbandRssiSamples         = RFID_NARROWBAND_RSSI_BASE_SAMPLES << MAC_RFTC_PROF_RSSIAVECFG_GET_NORM_NBSAMPS(registerValue);
		    pProfileInfo->realtimeRssiEnabled           = MAC_RFTC_PROF_RSSIAVECFG_RT_IS_ENABLED(registerValue);
		    pProfileInfo->realtimeWidebandRssiSamples   = MAC_RFTC_PROF_RSSIAVECFG_GET_RT_WBSAMPS(registerValue);
		    pProfileInfo->realtimeNarrowbandRssiSamples = MAC_RFTC_PROF_RSSIAVECFG_GET_RT_NBSAMPS(registerValue);

		    switch (pProfileInfo->profileProtocol)
		    {
		        case RFID_RADIO_PROTOCOL_ISO18K6C:
			    {
				    RFID_RADIO_LINK_PROFILE_ISO18K6C_CONFIG* pConfig = &(pProfileInfo->profileConfig.iso18K6C);

				    pConfig->length             = sizeof(RFID_RADIO_LINK_PROFILE_ISO18K6C_CONFIG);
				    pConfig->modulationType     = m_pMac->ReadRegister(MAC_RFTC_PROF_R2TMODTYPE);
				    pConfig->tari               = m_pMac->ReadRegister(MAC_RFTC_PROF_TARI);
				    pConfig->data01Difference   = m_pMac->ReadRegister(MAC_RFTC_PROF_X);
				    pConfig->pulseWidth         = m_pMac->ReadRegister(MAC_RFTC_PROF_PW);
				    pConfig->rtCalibration      = m_pMac->ReadRegister(MAC_RFTC_PROF_RTCAL);
				    pConfig->trCalibration      = m_pMac->ReadRegister(MAC_RFTC_PROF_TRCAL);
				    pConfig->divideRatio        = m_pMac->ReadRegister(MAC_RFTC_PROF_DIVIDERATIO);
				    pConfig->millerNumber       = m_pMac->ReadRegister(MAC_RFTC_PROF_MILLERNUM);
				    pConfig->trLinkFrequency    = m_pMac->ReadRegister(MAC_RFTC_PROF_T2RLINKFREQ);
				    pConfig->varT2Delay         = m_pMac->ReadRegister(MAC_RFTC_PROF_VART2DELAY);
				    pConfig->rxDelay            = m_pMac->ReadRegister(MAC_RFTC_PROF_RXDELAY);
				    pConfig->minT2Delay         = m_pMac->ReadRegister(MAC_RFTC_PROF_MINTOTT2DELAY);
				    pConfig->txPropagationDelay = m_pMac->ReadRegister(MAC_RFTC_PROF_TXPROPDELAY);
				    break;
			    } // case RFID_RADIO_PROTOCOL_ISO18K6C
		        
                default:
				    break;
		}
#endif
	}
        
        
        // Attempt to load all link profiles currently on the radio
        // keeping track of the profile marked active

        public Result Load( )
        {
            this.Clear( );

            UInt32 profileIndex = 0;

            while ( true )
            {
                RadioLinkProfile profile = new RadioLinkProfile( );

                Result status = Result.OK;

                //                    this.transport.RadioGetLinkProfile(this.radioIndex, profileIndex, profile );
                GetLinkProfile (profileIndex, profile);

                if (Result.OK == status)
                {
                    this.Add( new LinkProfileInfo( profile ) );

                    if ( 0 != profile.enabled )
                    {
                        this.activeProfileIndex = ( Int32 ) profileIndex;                        
                    }                    
                }
                else if (Result.INVALID_PARAMETER == status)
                {
                    break; // this rcv when profileIndex > profile count on radio
                }
                else if (Result.EMULATION_MODE == status)
                {
                    break; // this rcv when library ( transport ) is in emulation mode
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine( "Error while reading radio link profiles" );
                    return status; // this rcv all other errors
                }

                ++ profileIndex;
                if (MAX_LINK_PROFILE < profileIndex)
                    break;
            }

            return Result.OK;
        }


        // Attempt to save all link profiles currently on the radio
        // and mark the active one.
        /*
        public Result store()
        {
            // In reality we can only set the active profile index at this
            // time so devolution to a single call of:

            Result status =
                this.transport.RadioSetCurrentLinkProfile( this.readerHandle, ( UInt32 ) this.activeProfileIndex );

            return status;
        }*/

        /*
        // Attempt to locate matching link profile where uniqueId consists of
        // the string [profileId]:[profileVersion]

        public Source_LinkProfile FindByUniqueId( String uniqueIdString )
        {
            if ( null == uniqueIdString )
            {
                return null;
            }

            string[ ] parts = uniqueIdString.Split( ':' );

            if ( null == parts || 2 != parts.Length )
            {
                return null;
            }

            UInt64 profileId;
            UInt32 profileVersion;

            if ( ! ( UInt64.Parse( parts[ 0 ], out profileId ) && UInt32.Parse( parts[ 1 ], out profileVersion ) ) )
            {
                return null;
            }

            return this.FindByUniqueId( profileId, profileVersion );
        }


        // Attempt to locate and reture a link profile with a matching
        // unique id ( profileId & profileVersion )

        public Source_LinkProfile FindByUniqueId( UInt64 profileId, UInt32 profileVersion )
        {
            Source_LinkProfile Result = this.Find
                (
                    delegate( Source_LinkProfile p )
                    {
                        return p.ProfileId == profileId && p.ProfileVersion == profileVersion;
                    }
                );

            return Result;
        }

        */
        // Retrieve active profile ( object ) from cached info ~ if guaranteed up to
        // date info required, perform a load( ) operation immediately prior

        public LinkProfileInfo getActiveProfile( )
        {
            return this[ this.activeProfileIndex ];
        }

        public LinkProfileInfo getActiveProfile(uint index)
        {
            return this[(Int32)index];
        }

        // Retrieve active profile index from cached info ~ if guaranteed up to
        // date info required, perform a load( ) operation immediately prior

        public UInt32 getActiveProfileIndex( )
        {
            return ( UInt32 ) this.activeProfileIndex;
        }


        public void setActiveProfileIndex( int index )
        {
            this[ this.activeProfileIndex ].Enabled = false;
            this.activeProfileIndex = index;
            this[ this.activeProfileIndex ].Enabled = true;
        }


    } // End class Source_LinkProfileList


} // End namespace RFID.RFIDInterface
