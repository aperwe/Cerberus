<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="html" indent="yes"/>
  <xsl:variable name="productname"><xsl:text disable-output-escaping="yes">OSLEBot</xsl:text></xsl:variable>
  <xsl:variable name="version"><xsl:text disable-output-escaping="yes">1.0</xsl:text></xsl:variable>
  <!--if extendedOutput is 1 then property values are listed. otherwise only LSResID property is listed.-->
  <xsl:variable name="extendedOutput">1</xsl:variable>
  <!--list of propeties to be listed in output. LSResID is always displayed-->
  <xsl:variable name="propsToList">
    <i>SourceString</i>
    <i>TargetString</i>
    <i>PseudoString</i>
    <i>Comments</i>
    <i>AutoComment</i>
    <i>UserSrcLocked</i>
    <i>UserTransLocked</i>
    <i>WordCount</i>
    <i>FileName</i>
    <i>LocalizationStatus</i>
    <i>TranslationOrigin</i>
  </xsl:variable>
  <xsl:template match="/">
    
    <xsl:variable name="totalCOcount" select="count(//co)"/>
    <html>
      <header>
        <title>
          <xsl:value-of select="$productname"/> output version <xsl:value-of select="$version"/>
        </title>
      </header>
      <body>
        <h1>
          <xsl:value-of select="$productname"/> output version <xsl:value-of select="$version"/>
        </h1>
        Total classified objects: <xsl:value-of select="$totalCOcount"/>.<br/>
        <h2>
          Activated rules per LocItem
        </h2>
        <br/>
        <xsl:if test="not(function-available('msxsl:node-set'))">
          <B>
            <font size="+1" color="red">
            WARNING: THIS BROWSER WILL NOT DISPLAY PROPERTY VALUES FOR LocResource OBJECTS.<br/>
              USE INTERNET EXPLORER FOR BEST RESULTS.
            </font>
          </B>
        </xsl:if>
        <xsl:variable name="rules" select="''"/>
        <ol>
          <xsl:for-each select="//co">
            <xsl:variable name ="co" select ="."/>
            <li>
              <xsl:value-of select="$co/props/property[@key='LSResID']"/><br/>
              <xsl:if test="function-available('msxsl:node-set')">
                <xsl:if test="$extendedOutput = 1">
                  <xsl:for-each select="msxsl:node-set($propsToList)/i">
                  <xsl:variable name="prop" select="."/>
                  <b><xsl:value-of select="$prop"/>:</b>
                  <xsl:value-of select="$co/props/property[@key=$prop]"/>
                  <br/>
                </xsl:for-each>
                </xsl:if>
              </xsl:if>
              [<b>Rules</b>: <xsl:call-template name="ActivatedRuleNames"/>]
            </li>
          </xsl:for-each>
        </ol>
      </body>
    </html>
  </xsl:template>
  <xsl:template name="ActivatedRuleNames" match="co">
    <!--This template outputs sequence of rule names that have result="True" set. -->
    <xsl:for-each select="./rules/rule[@result='True']">
      <BR/>
      <B><U><xsl:value-of select="@name"/></U></B>:
      <xsl:for-each select="item">
        <xsl:variable name="result" select = "@result"/>
        <xsl:choose>
          <xsl:when test="$result = 'True'">
            <font color="blue">
              <b>
                <i>
                  <xsl:value-of select="@message"/>
                </i>-<b>
                  <xsl:value-of select="@result"/>;
                </b>
              </b>
            </font>
          </xsl:when>
          <xsl:otherwise>
            <font color="gray">
            <i>
              <xsl:value-of select="@message"/>
            </i>-<b>
              <xsl:value-of select="@result"/>;
            </b>
            </font>
          </xsl:otherwise>
        </xsl:choose>
    </xsl:for-each>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
