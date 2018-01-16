<?xml version="1.0"?>


<!-- Sample custom stylesheet created 01/05/2000 by avienneau-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">


  <xsl:template match="/">
    <HTML>


      <!-- The HEAD secton provides information that doesn't affect the content of
           the page. -->
      <HEAD>


        <!-- The STYLE block defines the formatting and text styles for this page. 
             The BODY style defines the defaults for the entire page. The following 
             style classes customize the BODY style, and can be applied to any HTML
             element on the page by setting its CLASS property. -->
        <STYLE>
          BODY {margin-left:0.25in; margin-right:0.4in; background-color:#FFFFFF; 
                font-family:Verdana, Arial, Helvetica; font-size:12; color:#666666}

          .title  {font-size:20; font-weight:bold; color:#0000AA; text-align:center}

          .subtitle  {font-size:14; font-weight:bold; color:#0000AA; text-align:center}

          .heading  {font-size:14; font-weight:bold; color:#0000CC}

          .attribute  {font-weight:bold; color:#0000CC; margin-left:0.25in}

          .property  {margin-left:0.5in}

          .description  {margin-left:0.75in; margin-right:0.25in}

          .separator  {text-align:center; color:#6060FF}

        </STYLE>
      </HEAD>
	
      <BODY>


        <!-- TITLE and DATA FORMAT. Add the title and data format, centered at the
             top of the page, using the title and subtitle styles. --> 
        <DIV CLASS="title">
          <xsl:value-of select="metadata/idinfo/citation/citeinfo/title"/> 
        </DIV>
        <DIV CLASS="subtitle">
          <xsl:value-of select="metadata/idinfo/natvform"/> 
        </DIV>
        <BR/><BR/>


        <!-- DETAILED. There is one detailed element in the metadata for each entity 
             in the data source. Some geographic data may not have any attributes. This 
             xsl:for-each selects detailed elements that contain attribute elements. --> 
        <xsl:for-each select="metadata/eainfo/detailed[attr]">


          <!-- ENTITY NAME. Add a heading with the name of the entity described by 
               the currently selected detailed element. The text "Attributes of:"
               is presented with the heading style; the name is presented with the
               default BODY text. --> 
          <DIV>
	    <SPAN CLASS="heading">Attributes of: </SPAN>
            <xsl:value-of select="enttyp/enttypl"/> 
          </DIV>
          <BR/>


          <!-- ATTRIBUTES. All attr elements within the current detailed element are 
               selected. The stylesheet loops through them, and for each one applies 
               the attr template, which is defined below. The attr template is 
               analogous to a subroutine in a program. --> 
          <xsl:apply-templates select="attr"/>


          <!-- SEPARATOR. Before looping to the next detailed element, check if the 
               current detailed element is the last in the set using the context 
               element. If it's not the last, add a line and a couple spaces to 
               separate the two attribute lists. The line is created using a centered 
               group of underscore characters. --> 
<!--           <xsl:if test="context()[not(end())]">
            <DIV CLASS="separator">__________________</DIV>
            <BR/><BR/>
          </xsl:if> -->


        </xsl:for-each> <!-- loop to next detailed element --> 

        <BR/><BR/>
      </BODY>
    </HTML>
  </xsl:template>


  <!-- ATTRIBUTE TEMPLATE. The contents of this template work like the contents of 
       a for-each loop. For each attribute, add its properties to the page.  --> 
  <xsl:template match="metadata/eainfo/detailed/attr">


    <!-- NAME. The attribute name is a heading for the properties listed below. The 
         xsl:choose element tests if there is an attribute name, and if so adds it 
         to the page. Otherwise, the text "Attribute" is added as the heading. --> 
    <xsl:choose>
      <xsl:when test="attrlabl[. != '']">
        <DIV CLASS="attribute"><xsl:value-of select="attrlabl"/></DIV>
      </xsl:when>
      <xsl:otherwise>
        <DIV CLASS="attribute">Attribute</DIV>
      </xsl:otherwise>
    </xsl:choose>


    <!-- PROPERTIES. For each property, if the tag is present and if it has a value, 
         add a label identifying the property then add its value to the page. --> 
    <xsl:if test="attrtype[. != '']">
      <DIV CLASS="property">
        <SPAN STYLE="color:#6060FF">Data type: </SPAN>
        <xsl:value-of select="attrtype"/><BR/>
      </DIV>
    </xsl:if>

    <xsl:if test="attwidth[. != '']">
      <DIV CLASS="property">
        <SPAN STYLE="color:#6060FF">Width: </SPAN>
        <xsl:value-of select="attwidth"/><BR/>
      </DIV>
    </xsl:if>
  
    <xsl:if test="atoutwid[. != '']">
      <DIV CLASS="property">
        <SPAN STYLE="color:#6060FF">Display width: </SPAN>
        <xsl:value-of select="atoutwid"/><BR/>
      </DIV>
    </xsl:if>

    <xsl:if test="atprecis[. != '']">
      <DIV CLASS="property">
        <SPAN STYLE="color:#6060FF">Precision: </SPAN>
        <xsl:value-of select="atprecis"/><BR/>
      </DIV>
    </xsl:if>

    <xsl:if test="attscale[. != '']">
      <DIV CLASS="property">
        <SPAN STYLE="color:#6060FF">Scale: </SPAN>
        <xsl:value-of select="attscale"/><BR/>
      </DIV>
    </xsl:if>

    <xsl:if test="atnumdec[. != '']">
      <DIV CLASS="property">
        <SPAN STYLE="color:#6060FF">Number of decimals: </SPAN>
        <xsl:value-of select="atnumdec"/><BR/>
      </DIV>
    </xsl:if>

    <xsl:if test="attrdef[. != '']">
      <DIV CLASS="property">
        <SPAN STYLE="color:#6060FF">Description: </SPAN>
      </DIV>
      <DIV CLASS="description">
        <xsl:value-of select="attrdef"/><BR/>
      </DIV>
    </xsl:if>


    <!-- SPACE. If the current attribute is not the last in the set, add an empty 
         line before the next attribute is added to the page. --> 
<!--     <xsl:if test="context()[not(end())]">
      <BR/>
    </xsl:if> -->

  </xsl:template>
	
</xsl:stylesheet>
