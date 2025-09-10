<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output method="xml" indent="yes" encoding="utf-8"/>
    
    <xsl:template match="/">
        <Employees>
            <xsl:for-each select="//item[not(@name = preceding::item/@name and @surname = preceding::item/@surname)]">
                <xsl:variable name="currentName" select="@name"/>
                <xsl:variable name="currentSurname" select="@surname"/>
                
                <Employee name="{$currentName}" surname="{$currentSurname}">
                    <xsl:for-each select="//item[@name = $currentName and @surname = $currentSurname]">
                        <xsl:variable name="correctMount">
                            <xsl:choose>
                                <!-- Если mount не соответствует родительскому элементу, исправляем -->
                                <xsl:when test="name(..) != 'Pay' and name(..) != @mount">
                                    <xsl:value-of select="name(..)"/>
                                </xsl:when>
                                <xsl:otherwise>
                                    <xsl:value-of select="@mount"/>
                                </xsl:otherwise>
                            </xsl:choose>
                        </xsl:variable>
                        <salary amount="{@amount}" mount="{$correctMount}"/>
                    </xsl:for-each>
                </Employee>
            </xsl:for-each>
        </Employees>
    </xsl:template>
</xsl:stylesheet>