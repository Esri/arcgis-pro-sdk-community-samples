<?xml version="1.0"?>  <!-- this is an xml file -->


<!-- Sample custom stylesheet created 01/05/2000 by avienneau-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">


  <!-- When the root node in the XML is encountered (the metadata element), the  
       HTML template is set up. -->
  <xsl:template match="/">
    <HTML>

	
      <!-- The BODY secton defines the content of the HTML page. This page has a 1/4 
           inch margin, a background color, and the default text size used is 13pt. -->
      <BODY STYLE="margin-left:0.25in; margin-right:0.25in; background-color:#FFF8DC; 
                   font-size:13pt">


        <!-- TITLE. The xsl:value-of element selects the title element in the XML 
             file, then places its data inside a DIV element. Because an XSL stylesheet 
             is an XML file, all XSL and HTML elements must be well-formed; that is, 
             they must be properly closed. The /DIV closes the opening division tag. 
             The value-of and break (BR) elements are closed by adding the / at the end. --> 
        <DIV STYLE="font-size:24; font-weight:bold; color:#8B0000; 
                    text-decoration:underline; text-align:center">
          <xsl:value-of select="metadata/idinfo/citation/citeinfo/title"/>
        </DIV>
        <BR/>

      
        <!-- AUTHOR. Add the question text, then place each author on a new line. The 
             xsl:for-each element loops through each origin element in the metadata, 
             and for each one adds a DIV element to the page. The xsl:value-of 
             element places the data in the currently selected origin tag inside. --> 
        <DIV STYLE="font-weight:bold; color:#B22222">
          Who created this data?
        </DIV>
        <xsl:for-each select="metadata/idinfo/citation/citeinfo/origin">
          <DIV STYLE="margin-left:0.25in; color:#696969">
            <xsl:value-of select="metadata/idinfo/citation/citeinfo/origin"/>
          </DIV>
        </xsl:for-each>
        <BR/>

        <DIV STYLE="font-weight:bold; color:#B22222">
          When was this data created?
        </DIV>
          <DIV STYLE="margin-left:0.25in; color:#696969">
            <xsl:value-of select="substring(metadata/Esri/CreaDate,5,2)"/>/<xsl:value-of select="substring(metadata/Esri/CreaDate,7,2)"/>/<xsl:value-of select="substring(metadata/Esri/CreaDate,1,4)"/>
          </DIV>
        <BR/>

        <DIV STYLE="font-weight:bold; color:#B22222">
          When was this data last modified?
        </DIV>
          <DIV STYLE="margin-left:0.25in; color:#696969">
            <xsl:value-of select="substring(metadata/Esri/ModDate,5,2)"/>/<xsl:value-of select="substring(metadata/Esri/ModDate,7,2)"/>/<xsl:value-of select="substring(metadata/Esri/ModDate,1,4)"/>
          </DIV>
        <BR/>

        <!-- ABSTRACT. Add the question and abstract data to the page. If the 
             metadata doesn't have an abstract element or if it contains no data, 
             no text appears below the question. -->
        <DIV STYLE="font-weight:bold; color:#B22222">
          What does this data represent?
        </DIV>
        <DIV STYLE="margin-left:0.25in; color:#696969">
          <xsl:value-of select="metadata/idinfo/descript/abstract"/>
        </DIV>
        <BR/>


        <!-- EXTENT. The bounding tag is selected using xsl:for-each; if it exists in 
             the metadata, labels for each coordinate are placed on separate lines with  
             bold text. The coordinate values are retrieved from the appropriate 
             tags and placed next to the appropriate labels. -->
        <DIV STYLE="font-weight:bold; color:#B22222">
          What is the data's extent?
        </DIV>
        <DIV STYLE="margin-left:0.25in; color:#696969">
          <xsl:for-each select="metadata/idinfo/spdom/bounding">
            <SPAN STYLE="font-weight:bold">North: </SPAN><xsl:value-of select="northbc"/><BR/>
            <SPAN STYLE="font-weight:bold">South: </SPAN><xsl:value-of select="southbc"/><BR/>
            <SPAN STYLE="font-weight:bold">East: </SPAN><xsl:value-of select="eastbc"/><BR/>
            <SPAN STYLE="font-weight:bold">West: </SPAN><xsl:value-of select="westbc"/>
          </xsl:for-each>
        </DIV>
        <BR/>


        <!-- COORDINATE SYSTEM. These xsl:if elements test whether or not the 
             coordinate system name elements exist within the metadata and if they 
             contain a value. If so, an appropriate label and the coordinate system 
             name are placed on the page. -->
        <DIV STYLE="font-weight:bold; color:#B22222">
          What is the data's coordinate system?
        </DIV>
        <xsl:if test="metadata/spref/horizsys/cordsysn/projcsn[. != '']">
          <DIV STYLE="margin-left:0.25in; color:#696969">
            <SPAN STYLE="font-weight:bold">Projected: </SPAN>
            <xsl:value-of select="metadata/spref/horizsys/cordsysn/projcsn"/>
          </DIV>
        </xsl:if>
        <xsl:if test="metadata/spref/horizsys/cordsysn/geogcsn[. != '']">
          <DIV STYLE="margin-left:0.25in; color:#696969">
            <SPAN STYLE="font-weight:bold">Geographic: </SPAN>
            <xsl:value-of select="metadata/spref/horizsys/cordsysn/geogcsn"/>
          </DIV>
        </xsl:if>

        <BR/><BR/>
      </BODY>

    </HTML>
  </xsl:template>
	
</xsl:stylesheet>